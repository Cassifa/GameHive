# GameHive Multi-Agent 协作博弈决策系统 — 架构设计文档

> 本文档描述 `multi-agent` 模块的完整架构设计，基于《多Agent协作系统完整技术文档》的核心概念，为棋类博弈场景量身定制。

---

## 一、术语定义（对标技术文档）

| 术语 | 定义 | 对标文档实体 | 本项目 Java 实体 |
|------|------|-------------|-----------------|
| **Host Agent（主控 Agent）** | 博弈会话的执行主体，负责组织 LLM 回合、调用工具、委派子 Agent | `Agent`（`base_agent.ts`） | `GameHostAgent` |
| **POC Agent（战略总监）** | 可切换主控角色，通过 handoff 将控制权转移给另一个 POC Agent | `PocAgentManager` | `PocAgentManager` |
| **Task Agent（专家子 Agent）** | 被主控 Agent 以工具调用方式创建，完成独立子任务后返回结果 | `TaskAgentManager` | `TaskAgentManager` |
| **Tool（工具）** | LLM 输出的结构化指令，由后端执行并回填结果到对话上下文 | `ChatMCPManager` | `ToolManager` |
| **Hook（消息钩子）** | 在 RunLoop 不同阶段对消息/流程做强制改写、校验或短路 | `AIMessageHookManager` | `HookManager` |
| **GameWorkspace（共享博弈状态）** | 跨 Agent 共享的结构化博弈状态，修改必须走事务与锁 | `WorkspaceDocument` | `GameWorkspace` |
| **RunLoop（运行循环）** | Agent 的核心调度闭环：准备上下文→调用LLM→处理响应→执行工具→判断继续 | `Agent.runLoop()` | `AgentRunLoop` |
| **Executor Ownership（执行权）** | 同一博弈会话只允许一个 executor 继续推进 | `checkExecutorOwnership()` | `GameSession.checkOwnership()` |

---

## 二、系统架构概述

### 2.1 架构模式

采用 **Supervisor + Hierarchical Handoff + Sequential Sub-Agent** 混合协作模式：

| 层级 | 模式 | 参与 Agent | 通信方式 |
|------|------|------------|----------|
| **第一层** | Peer-to-Peer | POC Agent 间 | `handoff_to_agent` 工具调用 |
| **第二层** | Supervisor-Worker | POC Agent → Task Agent | `call_task_agent` 工具调用 |
| **控制层** | Workflow-Driven | Hook 引擎 → 所有 Agent | Hook 机制 + GameWorkspace |

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Workflow Controller（控制层）                     │
│   (GamePhaseHook + UndoHook + HookManager + GameWorkspace)          │
└────────────────────────────┬─────────────────────────────────────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
      ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
      │  POC Agent  │◄►│  POC Agent  │◄►│  POC Agent  │
      │ (战略总监)   │ │ (战术协调员) │ │ (残局专家)   │
      └──────┬──────┘ └──────┬──────┘ └──────┬──────┘
             │               │               │
      ┌──────┼──────┐       │         ┌─────┼──────┐
      ▼      ▼      ▼       ▼         ▼            ▼
  ┌───────┐┌───────┐┌───────┐┌───────┐┌───────┐┌───────┐┌───────┐
  │ Task  ││ Task  ││ Task  ││ Task  ││ Task  ││ Task  ││ Task  │
  │ Agent ││ Agent ││ Agent ││ Agent ││ Agent ││ Agent ││ Agent │
  │(规则  ││(开局  ││(攻势  ││(防守  ││(局部  ││(收官  ││(悔棋  │
  │ 分析) ││ 专家) ││ 分析) ││ 分析) ││ 战术) ││ 计算) ││ 重评) │
  └───────┘└───────┘└───────┘└───────┘└───────┘└───────┘└───────┘
```

### 2.2 Agent 类型对比

| 类型 | 职责 | 生命周期 | RunLoop 上限 |
|------|------|----------|-------------|
| **POC Agent（战略总监）** | 理解棋局全局态势，决定战略方向，协调子 Agent | 长生命周期，可 Handoff | 30 次 |
| **POC Agent（战术协调员）** | 中盘阶段的局部战术规划与执行 | 长生命周期，可 Handoff | 30 次 |
| **POC Agent（残局专家）** | 残局阶段精确计算与收官 | 长生命周期，可 Handoff | 30 次 |
| **Task Agent（各专家）** | 完成特定分析子任务（开局检索、攻防分析、悔棋重评等） | 短生命周期，执行完销毁 | 15 次 |

### 2.3 与原架构的核心差异

| 维度 | 原 lmmrunningsystem | 新 multi-agent |
|------|---------------------|----------------|
| Agent 结构 | 同质 Agent × 5，仅 temperature 不同 | 分层异构：POC + Task，各有专属 prompt |
| 决策方式 | 平行投票（多数决） | 层级协作（主控综合专家意见） |
| 状态管理 | InMemoryChatMemory | GameWorkspace 事务 + 锁 |
| 流程控制 | 无 | Hook 机制 + RunLoop |
| 棋局阶段感知 | 无 | GamePhaseHook 自动识别 |
| 悔棋支持 | 无 | UndoHook + 悔棋重评 Task Agent |
| 异常处理 | 简单重试 | 循环保护 + 重复检测 + 熔断 |
| 可观测性 | log.info | 结构化 Trace + Span 埋点 |

---

## 三、核心组件详解

### 3.1 AgentRunLoop（运行循环）— 对标 `Agent.runLoop()`

```
┌─────────────────────────────────────────────────────────┐
│                    AgentRunLoop                          │
│                                                          │
│  while (loopCount <= maxLoopCount) {                     │
│      1. checkExecutorOwnership()     // 检查执行权        │
│      2. hookManager.execute(LOOP_START)  // 前置 Hook    │
│      3. buildLLMContext()            // 组装上下文        │
│      4. callLLM()                    // 调用大模型        │
│      5. hookManager.execute(PROCESS_RESPONSE) // 响应Hook│
│      6. if (hasToolCalls) {                               │
│             toolManager.invokeTool() // 执行工具调用      │
│             hookManager.execute(LOOP_END_WITH_TOOL)      │
│             continue;                                     │
│         }                                                 │
│      7. hookManager.execute(LOOP_END_WITH_MESSAGE)       │
│      8. break; // 无工具调用，结束循环                     │
│  }                                                        │
└─────────────────────────────────────────────────────────┘
```

**关键设计：**
- **最大循环次数**：POC Agent 30 次，Task Agent 15 次（防止无限循环）
- **执行权检查**：每轮检查 `GameSession.checkOwnership()`，防止多 executor 同时推进
- **中断机制**：支持外部中断信号（用户悔棋、超时等）

### 3.2 ToolManager（工具管理器）— 对标 `ChatMCPManager`

统一路由所有工具调用：

```java
public ToolResult invokeTool(String toolName, Map<String, Object> args) {
    switch (toolName) {
        case "call_task_agent":
            return taskAgentManager.invokeCallTaskAgent(args);
        case "handoff_to_agent":
            return pocAgentManager.invokeHandoffToAgent(args);
        case "evaluate_position":
            return evaluatePosition(args);
        case "search_pattern":
            return searchPattern(args);     // RAG 检索棋型
        case "analyze_board":
            return analyzeBoard(args);
        case "undo_evaluate":
            return undoEvaluate(args);      // 悔棋后重新评估
        default:
            throw new ToolNotFoundException(toolName);
    }
}
```

**内建工具清单：**

| 工具名 | 类型 | 说明 |
|--------|------|------|
| `call_task_agent` | 协作 | 委派子任务给 Task Agent |
| `handoff_to_agent` | 协作 | 将主控权转移给另一个 POC Agent |
| `analyze_rule` | 规则 | 深度解读游戏规则，输出 RuleInterpretation（首回合由 Hook 强制触发） |
| `evaluate_position` | 分析 | 评估特定位置的得分 |
| `search_pattern` | RAG | 在知识库中检索匹配棋型 |
| `analyze_board` | 分析 | 分析当前棋盘全局态势 |
| `undo_evaluate` | 悔棋 | 悔棋后重新评估棋局 |
| `get_workflow_guide` | 控制 | Hook 注入的伪工具，携带控制指令 |

### 3.3 HookManager（消息钩子管理器）— 对标 `AIMessageHookManager`

#### 3.3.1 Hook 生命周期阶段

```java
public enum HookPhase {
    BEFORE_LOOP,                // 请求到达后，进入第一次 loop 前
    LOOP_START,                 // 每次 loop 开始
    LOOP_PROCESS_LLM_RESPONSE,  // 每次 loop 处理 LLM 响应后
    LOOP_END_WITH_TOOL_CALL,    // loop 结束时有工具调用
    LOOP_END_WITH_MESSAGE       // loop 结束时有消息（无工具调用）
}
```

#### 3.3.2 Hook 执行流程

```
请求到达
    │
    ▼
┌──────────────────────┐
│  for (hook : hooks)  │
└──────────┬───────────┘
           │
           ▼
    hook.shouldExecute(context)?
           │
    ┌──────┴──────┐
    │ false       │ true
    ▼             ▼
  跳过       hook.execute(context)
                  │
                  ▼
          result.continueToNextHook?
                  │
           ┌──────┴──────┐
           │ true        │ false
           ▼             ▼
       继续下一个      短路返回（结果合并后立即返回）
```

#### 3.3.3 完整 Hook 注册列表

```java
private final List<GameHook> hooks = List.of(
    // === 输入校验层 ===
    new InputValidationHook(),          // 校验请求合法性

    // === 悔棋处理层 ===
    new UndoDetectionHook(),            // 检测是否为悔棋请求
    new UndoExecutionHook(),            // 执行悔棋状态回滚

    // === 规则理解层 ===
    new RuleAnalysisHook(),             // 首回合强制调用规则分析 Task Agent

    // === 棋局阶段识别层 ===
    new GamePhaseDetectionHook(),       // 识别开局/中盘/残局阶段

    // === 循环防护层 ===
    new LoopProtectionHook(),           // 重复 Handoff / ToolCall 检测
    new RepeatMoveDetectionHook(),      // 检测重复落子建议

    // === 落子校验层 ===
    new MoveValidationHook(),           // 校验 LLM 输出的落子合法性
    new MoveRetryHook(),                // 非法落子时注入纠正指令

    // === 上下文管理层 ===
    new ContextCompressionHook(),       // 消息数 > 阈值时触发压缩
    new MemorySummaryHook(),            // Handoff 时生成记忆摘要

    // === 工具过滤层 ===
    new ToolFilterHook(),               // 根据控制指令过滤可用工具

    // === 工作流控制层 ===
    new WorkflowPhaseTransitionHook(),  // 棋局阶段变化时强制 Handoff
    new WorkflowGuideHook()             // 注入 get_workflow_guide 伪工具消息
);
```

### 3.4 GameWorkspace（共享博弈状态）— 对标 `WorkspaceDocument`

#### 3.4.1 数据结构

```java
public class GameWorkspace {
    // === 博弈核心状态 ===
    private String gameId;                    // 游戏唯一标识
    private String gameType;                  // 游戏类型
    private String gameRule;                  // 游戏规则
    private int gridSize;                     // 棋盘大小
    private String[][] boardState;            // 当前棋盘状态
    private List<MoveRecord> moveHistory;     // 完整落子历史

    // === 工作流状态 ===
    private GamePhase currentPhase;           // 当前阶段：OPENING/MIDGAME/ENDGAME
    private String currentAgentName;          // 当前执行的 POC Agent
    private boolean gameEnded;                // 游戏是否结束

    // === 规则理解状态 ===
    private RuleInterpretation ruleInterpretation; // 规则分析 Task Agent 的输出
    //   包含：胜负条件解读、关键棋型定义、特殊规则注意事项、易混淆点

    // === 分析状态（跨 Agent 共享黑板）===
    private ThreatAnalysis threatAnalysis;    // 威胁分析结果
    private StrategicPlan strategicPlan;      // 当前战略计划
    private List<CandidateMove> candidateMoves; // 候选走法及评分
    private String ragContext;                // RAG 检索到的知识上下文

    // === 悔棋状态 ===
    private UndoState undoState;              // 悔棋状态信息
    private List<WorkspaceSnapshot> snapshots;// 状态快照栈（用于悔棋回滚）

    // === 变更队列 ===
    private List<WorkspaceChange> changeList; // 结构化变更队列

    // === 特性开关 ===
    private Set<String> featureFlags;         // 特性开关集合
}
```

#### 3.4.2 事务与锁机制 — 对标 `executeWorkspaceTransaction()`

```java
public class WorkspaceTransaction {
    private final ReentrantLock lock = new ReentrantLock();
    private static final long LOCK_TIMEOUT_MS = 30_000; // 30秒超时

    public GameWorkspace executeTransaction(
            String gameId,
            Function<GameWorkspace, GameWorkspace> modifier
    ) {
        if (!lock.tryLock(LOCK_TIMEOUT_MS, TimeUnit.MILLISECONDS)) {
            throw new WorkspaceLockTimeoutException(gameId);
        }
        try {
            // 1. 读取旧状态
            GameWorkspace old = loadWorkspace(gameId);
            // 2. 深拷贝并修改
            GameWorkspace updated = deepCopy(old);
            updated = modifier.apply(updated);
            // 3. 计算 diff
            WorkspaceDiff diff = calculateDiff(old, updated);
            if (diff.isEmpty()) return old;
            // 4. 持久化
            saveWorkspace(gameId, updated);
            // 5. 记录变更
            logChange(gameId, diff);
            return updated;
        } finally {
            lock.unlock();
        }
    }
}
```

### 3.5 TaskAgentManager — 对标 `TaskAgentManager`

```
┌──────────────┐    call_task_agent     ┌────────────────────┐
│  POC Agent   │ ───────────────────────►│  TaskAgentManager  │
│ (战略总监)    │                         └──────────┬─────────┘
└──────────────┘                                    │
                                                    ▼
                                         ┌────────────────────┐
                                         │  构建子 Agent 上下文 │
                                         │  (prompt + 棋局状态) │
                                         └──────────┬─────────┘
                                                    │
                                                    ▼
                                         ┌────────────────────┐
                                         │  new TaskAgent()    │
                                         │  .runLoop()         │
                                         │  (maxLoop=15)       │
                                         └──────────┬─────────┘
                                                    │
                                                    ▼
                                         ┌────────────────────┐
                                         │  返回 ToolResult    │
                                         │  给 POC Agent       │
                                         └────────────────────┘
```

### 3.6 PocAgentManager — 对标 `PocAgentManager`

```java
public ToolResult invokeHandoffToAgent(Map<String, Object> args) {
    String targetAgentName = (String) args.get("agentName");
    String summary = (String) args.get("currentAgentOutputSummary");

    // 1. 保存当前 Agent 记忆摘要
    memorySummary = generateMemorySummary(currentMessages);

    // 2. 切换 Agent 配置
    AgentConfig newConfig = findAgentConfig(targetAgentName);
    agent.switchAgent(newConfig);

    // 3. 更新 GameWorkspace
    workspaceTransaction.executeTransaction(gameId, workspace -> {
        workspace.setCurrentAgentName(targetAgentName);
        workspace.getChangeList().add(
            new WorkspaceChange("HANDOFF", currentAgentName + " → " + targetAgentName, summary)
        );
        return workspace;
    });

    return ToolResult.success("[HANDOFF] 已成功移交给 " + targetAgentName);
}
```

**Handoff 排除机制（防循环）：** 维护 `handoffExcludeAgents` 集合，最近 20 条消息中 handoff >= 4 次则触发熔断。

---

## 四、完整博弈工作流

### 4.1 标准决策流程（一次落子）

```
┌─────────────────────────────────────────────────────────────────┐
│                        请求到达                                   │
│   admin POST /LMMRunning/add/ → LMMPool → Consumer              │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   BEFORE_LOOP Hooks                              │
│   InputValidationHook → UndoDetectionHook                       │
│   (如果是悔棋请求，短路到悔棋流程)                                  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                   GamePhaseDetectionHook                         │
│   分析棋盘 → 判断阶段 → 写入 GameWorkspace.currentPhase           │
│   如果阶段变化 → 触发 handoff_to_agent 到对应 POC Agent           │
│                                                                  │
│   开局(回合<总格数10%) → 战略总监                                  │
│   中盘(10%~70%)       → 战术协调员                                │
│   残局(>70%)          → 残局专家                                  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│              POC Agent RunLoop（以战略总监为例）                    │
│                                                                  │
│  Loop 0: [首回合] 规则分析（Hook 强制）                             │
│    → call_task_agent("规则分析专家", "深度解读本局游戏规则")         │
│    → 输出 RuleInterpretation → 写入 GameWorkspace 共享给所有 Agent  │
│                                                                  │
│  Loop 1: 调用 LLM 分析棋局（system prompt 已注入规则解读）          │
│    → LLM 输出: call_task_agent("开局专家", "分析当前开局型")        │
│    → 开局专家 Task Agent 运行 → 返回开局知识                       │
│                                                                  │
│  Loop 2: 综合开局知识，继续分析                                    │
│    → LLM 输出: call_task_agent("攻势分析", "评估进攻机会")          │
│    → 攻势分析 Task Agent 运行 → 返回攻击候选                       │
│                                                                  │
│  Loop 3: 综合所有分析，做出最终决策                                │
│    → LLM 输出: {"x": 7, "y": 7, "reason": "..."}                │
│                                                                  │
│  MoveValidationHook 校验合法性                                    │
│    → 合法: 结束循环                                               │
│    → 非法: MoveRetryHook 注入纠正指令，继续 Loop                   │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                     结果回传                                      │
│   Consumer → RestTemplate.post → admin /api/pk/receive/LMM/move/│
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 悔棋流程（UNDO Workflow）

悔棋是本系统的新增核心功能。悔棋不由 admin 实际触发（admin 不会调用悔棋），而是在多 Agent 系统**内部**支持悔棋语义——当 Agent 检测到之前的决策导致局势恶化时，可以在内部"悔棋"重新评估。

#### 4.2.1 悔棋触发场景

| 触发方式 | 说明 |
|----------|------|
| **Agent 自省悔棋** | 当前 POC Agent 在分析中发现上一步决策存在严重失误 |
| **Hook 强制悔棋** | `UndoDetectionHook` 检测到对手落子后己方威胁大幅上升 |
| **Task Agent 建议悔棋** | 防守分析 Task Agent 发现不可逆的劣势 |

#### 4.2.2 悔棋状态机

```
┌─────────────┐  触发悔棋请求   ┌─────────────┐
│   正常博弈   │ ──────────────►│  悔棋检测    │
│  (PLAYING)   │                │ (UNDO_CHECK) │
└─────────────┘                └──────┬──────┘
                                      │
                         ┌────────────┴────────────┐
                         │                         │
                         ▼                         ▼
                  需要悔棋                    不需要悔棋
                         │                         │
                         ▼                         ▼
               ┌─────────────┐              返回正常流程
               │  状态回滚    │
               │(UNDO_ROLLBACK)│
               └──────┬──────┘
                      │
                      ▼
               ┌─────────────┐
               │  悔棋重评    │
               │(UNDO_REEVAL) │
               │ (Task Agent) │
               └──────┬──────┘
                      │
                      ▼
               ┌─────────────┐
               │  新决策生成   │
               │(UNDO_DECIDE) │
               └──────┬──────┘
                      │
                      ▼
               返回正常流程
```

#### 4.2.3 悔棋实现机制

```java
// UndoDetectionHook — BEFORE_LOOP 阶段检测
public class UndoDetectionHook implements GameHook {
    @Override
    public HookPhase getPhase() { return HookPhase.BEFORE_LOOP; }

    @Override
    public boolean shouldExecute(HookContext context) {
        // 条件1: 对手刚落子后，己方威胁值超过阈值
        // 条件2: GameWorkspace.undoState 标记了需要悔棋
        // 条件3: 上一步决策的事后评估分数极低
        return context.getWorkspace().getUndoState() != null
            && context.getWorkspace().getUndoState().isUndoRequested();
    }

    @Override
    public HookResult execute(HookContext context) {
        // 短路到悔棋流程
        return HookResult.builder()
            .continueToNextHook(false)
            .forceWorkflow("UNDO_WORKFLOW")
            .build();
    }
}

// UndoExecutionHook — 执行状态回滚
public class UndoExecutionHook implements GameHook {
    @Override
    public HookResult execute(HookContext context) {
        GameWorkspace workspace = context.getWorkspace();

        // 1. 从快照栈弹出上一状态
        WorkspaceSnapshot snapshot = workspace.getSnapshots().pop();

        // 2. 回滚棋盘状态
        workspace.setBoardState(snapshot.getBoardState());
        workspace.setMoveHistory(snapshot.getMoveHistory());
        workspace.setThreatAnalysis(snapshot.getThreatAnalysis());

        // 3. 标记悔棋完成，进入重评阶段
        workspace.getUndoState().setPhase(UndoPhase.UNDO_REEVAL);

        // 4. 强制调用悔棋重评 Task Agent
        return HookResult.builder()
            .continueToNextHook(true)
            .forceCallTool("call_task_agent")
            .forceCallArgs(Map.of("agentName", "悔棋重评专家",
                "prompt", "棋局已回滚，请重新分析当前局势并给出新的走法建议"))
            .build();
    }
}
```

#### 4.2.4 快照机制（支持悔棋回滚）

每次成功落子后，`GameWorkspace` 自动保存快照：

```java
// 在 WorkspaceTransaction 中自动保存快照
public GameWorkspace executeTransaction(..., Function<GameWorkspace, GameWorkspace> modifier) {
    // ... 执行修改 ...
    if (diff.containsMoveChange()) {
        // 保存落子前的快照（用于悔棋回滚）
        updated.getSnapshots().push(new WorkspaceSnapshot(old));
        // 限制快照栈深度（最多保留 N 步）
        if (updated.getSnapshots().size() > MAX_UNDO_DEPTH) {
            updated.getSnapshots().removeFirst();
        }
    }
    // ...
}
```

### 4.3 Handoff 流程（POC Agent 切换）

```
当前是"战略总监" POC Agent
    │
    │  GamePhaseDetectionHook 检测到棋局进入中盘
    │
    ▼
WorkflowPhaseTransitionHook 注入 handoff 指令
    │
    ▼
LLM 输出: handoff_to_agent("战术协调员", "棋局进入中盘，需要...")
    │
    ▼
PocAgentManager.invokeHandoffToAgent()
    │
    ├── 1. 生成当前 Agent 记忆摘要（MemorySummaryHook）
    │       → 调用 LLM 将对话压缩为 200 字以内摘要
    │
    ├── 2. switchAgent(newConfig)
    │       → 切换 system prompt、工具集、temperature
    │
    ├── 3. 更新 GameWorkspace
    │       → workspace.currentAgentName = "战术协调员"
    │       → workspace.changeList.add(HANDOFF 记录)
    │
    └── 4. 后续 RunLoop 在新 AgentConfig 下继续执行
```

### 4.4 Task Agent 委派流程

```
POC Agent (RunLoop 中)
    │
    │  LLM 输出: call_task_agent("防守分析专家", "分析对手最后一步的威胁")
    │
    ▼
TaskAgentManager.invokeCallTaskAgent()
    │
    ├── 1. 构建子上下文
    │       → 继承父 Agent 的棋局状态
    │       → 注入专家 system prompt
    │       → 传入 RAG 检索上下文
    │
    ├── 2. 创建 Task Agent
    │       → new TaskAgent(config, context, maxLoop=15)
    │
    ├── 3. 执行 Task Agent RunLoop
    │       → 可能多轮调用 LLM + 工具
    │       → 完成后销毁
    │
    └── 4. 返回 ToolResult 给 POC Agent
            → POC Agent 在下一个 Loop 中接收结果
            → 综合结果继续决策
```

---

## 五、棋局阶段工作流详解

### 5.1 阶段定义与转换

```java
public enum GamePhase {
    OPENING,    // 开局阶段
    MIDGAME,    // 中盘阶段
    ENDGAME     // 残局/收官阶段
}
```

**阶段判定规则（可配置）：**

| 游戏类型 | 开局 | 中盘 | 残局 |
|----------|------|------|------|
| 五子棋(15×15) | 回合 < 12 | 12 ≤ 回合 < 60 | 回合 ≥ 60 |
| 8×8五子棋 | 回合 < 6 | 6 ≤ 回合 < 30 | 回合 ≥ 30 |
| 不围棋(9×9) | 回合 < 8 | 8 ≤ 回合 < 40 | 回合 ≥ 40 |
| 井字棋(3×3) | 回合 < 2 | 2 ≤ 回合 < 5 | 回合 ≥ 5 |
| 反井字棋(3×3) | 回合 < 2 | 2 ≤ 回合 < 5 | 回合 ≥ 5 |

### 5.2 各阶段 Agent 协作

#### 5.2.1 开局阶段 → 战略总监 POC Agent

```
战略总监
    │
    ├── call_task_agent("规则分析专家")      ← 首回合强制调用
    │       → 深度解读游戏规则（胜负条件、关键棋型、特殊规则）
    │       → 输出结构化的 RuleInterpretation 写入 GameWorkspace
    │       → 后续所有 Agent 共享该规则解读，避免理解偏差
    │
    ├── call_task_agent("开局专家")
    │       → RAG 检索开局库（基于规则解读结果检索）
    │       → 返回推荐开局走法
    │
    ├── call_task_agent("攻势分析专家")
    │       → 评估先手/后手优劣
    │       → 返回初期布局建议
    │
    └── 综合分析 → 输出落子决策
```

**硬前置条件（Hook 强制）：**
- 若 `GameWorkspace.ruleInterpretation` 为空（首回合）→ **强制** `call_task_agent("规则分析专家")`，短路其他所有操作
- 若棋局刚开始且无历史步骤 → 强制 `call_task_agent("开局专家")`
- 若 RAG 检索到高匹配开局型 → 优先采纳开局库建议

**规则分析专家的输出结构（RuleInterpretation）：**

```java
public class RuleInterpretation {
    private String winCondition;           // 胜利条件解读
    private String loseCondition;          // 失败条件解读（对反向棋类尤其重要）
    private List<String> keyPatterns;      // 关键棋型定义（如"活三""冲四"）
    private List<String> specialRules;     // 特殊规则（如不围棋的"被围即得"）
    private List<String> commonMistakes;   // 易混淆点/常见错误理解
    private String strategicGuideline;     // 基于规则推导出的总体战略方向
}
```

> **设计意图**：不围棋（赢法是让对手围住你）、反井字棋（连成三子的输）等反向规则棋类，
> LLM 极易按常规规则理解导致策略完全反转。规则分析专家在首回合深度解读规则后写入共享状态，
> 后续所有 POC Agent 和 Task Agent 的 system prompt 中都会注入该解读，从根源避免理解偏差。

#### 5.2.2 中盘阶段 → 战术协调员 POC Agent

```
战术协调员
    │
    ├── call_task_agent("攻势分析专家")
    │       → 寻找进攻机会（连子、活棋等）
    │       → 返回攻击候选走法 + 评分
    │
    ├── call_task_agent("防守分析专家")
    │       → 识别对手威胁（冲四、活三等）
    │       → 返回防守紧急度 + 防守走法
    │
    ├── call_task_agent("局部战术专家")
    │       → 评估局部冲突区域
    │       → 返回战术组合走法
    │
    └── 综合攻防平衡 → 输出落子决策
```

**关键 Hook 策略：**
- 防守紧急度 > 阈值 → Hook 强制优先执行防守分析
- 攻防评分接近 → 多次调用不同 Task Agent 交叉验证

#### 5.2.3 残局阶段 → 残局专家 POC Agent

```
残局专家
    │
    ├── call_task_agent("收官计算专家")
    │       → 精确计算剩余空间
    │       → 返回最优收官序列
    │
    ├── call_task_agent("防守分析专家")
    │       → 最后的威胁检查
    │
    └── 精确计算 → 输出落子决策
```

### 5.3 记忆传递策略 — 对标 `IntentionMemoryManager`

| 场景 | 策略 | 说明 |
|------|------|------|
| Task Agent 调用 | 完整上下文 | Task Agent 继承父 Agent 的完整棋局上下文 |
| POC Agent Handoff | 记忆摘要 | 调用 LLM 将对话压缩为 200 字摘要，避免 Token 爆炸 |
| 上下文过长(>100 条消息) | 强制压缩 | `ContextCompressionHook` 触发，保留关键棋步和分析结论 |

---

## 六、异常处理与极端场景 — 对标文档第十章

### 6.1 Agent 行为异常

| 异常场景 | 处理策略 | 对标文档 |
|----------|----------|----------|
| **LLM 输出非法落子** | `MoveValidationHook` 校验 + `MoveRetryHook` 注入纠正指令，最多重试 3 次 | 工具参数解析失败处理 |
| **LLM 空输出** | 自动重试一次，仍失败则使用默认策略 | Gemini 空输出重试 |
| **Agent 陷入死循环** | `maxLoopCount` 限制（POC 30/Task 15），超限则输出当前最佳候选 | `maxLoopCount` 限制 |
| **重复 Handoff** | `LoopProtectionHook` 检测最近 20 条消息中 handoff ≥ 4 次则熔断 | `checkHasHandoffRepeatMessages()` |
| **重复 Tool Call** | `LoopProtectionHook` 检测最近 30 条消息中相同调用过多则报警 | `checkHasSameToolCallForRecentMessages()` |
| **JSON 解析失败** | 尝试 `jsonrepair` 修复，失败则重试 | `jsonrepair` 修复 |

### 6.2 协作流程异常

| 异常场景 | 处理策略 | 对标文档 |
|----------|----------|----------|
| **Handoff 循环** | 日志告警 + `handoffExcludeAgents` 排除 + 超限熔断 | Handoff 排除列表 |
| **Task Agent 失败** | 返回 `isError` ToolResult，POC 决策下一步 | Task Agent 失败处理 |
| **Handoff 目标不存在** | 返回固定文本错误，继续当前 Agent | 目标不存在处理 |
| **超时** | MCP 级超时 20 分钟 + 每步超时使用默认决策兜底 | MCP 超时 |
| **悔棋快照栈为空** | 跳过悔棋，正常决策 | 降级处理 |

### 6.3 并发与竞争

| 异常场景 | 处理策略 | 对标文档 |
|----------|----------|----------|
| **多 Agent 同时修改 GameWorkspace** | `WorkspaceTransaction` 锁队列串行化 + 30s 超时 | workspace 锁 |
| **锁超时** | 强制释放 + reject 等待者 | `forceReleaseLock` |
| **同一局多 executor** | 每轮 `checkOwnership()`，非 owner 退出 | `checkExecutorOwnership()` |

### 6.4 工具过滤协议 — 对标文档 7.5

Hook 通过 `ToolMessage.additionalKwargs` 传递控制指令：

| 字段 | 语义 |
|------|------|
| `forceCallTool` | 强制只能调用指定工具 |
| `useTools` | 可用工具白名单，或 `"never"` 禁用所有 |
| `hookUniqueKey` | Hook 唯一标识，用于重试计数 |
| `currentRetryCount` | 当前重试次数 |
| `maxRetryCount` | 最大重试次数，超限进入 FIX_MODE |

---

## 七、可观测性设计 — 对标文档第十一章

### 7.1 Trace 系统

| Span 名称 | 覆盖阶段 |
|-----------|----------|
| `agent.init` | Agent 初始化 |
| `agent.switch_agent` | POC Agent 切换 |
| `agent.run_loop_prepare` | RunLoop 准备阶段 |
| `agent.run_loop_llm_turn` | LLM 回合 |
| `agent.tool_call` | 工具调用（含 Task Agent） |
| `agent.hook_execute` | Hook 执行 |
| `agent.undo_rollback` | 悔棋回滚 |
| `agent.final_decision` | 最终决策输出 |

### 7.2 关键日志点

| 日志级别 | 场景 | 内容 |
|----------|------|------|
| DEBUG | 模型输入 | System Prompt + 棋局状态 + 最新用户消息 |
| DEBUG | 模型输出 | 输出内容 + 推理内容 + 工具调用 |
| INFO | 阶段切换 | 棋局阶段变化 + Handoff 详情 |
| INFO | 决策结果 | 最终落子坐标 + 理由 + 耗时 |
| WARN | 重复 Handoff | 检测到异常的重复 handoff_to_agent 调用 |
| WARN | 非法落子重试 | 落子校验失败 + 重试次数 |
| ERROR | 循环超限 | RunLoop 循环次数超限 |
| ERROR | 工具调用失败 | 工具名 + 参数 + 错误信息 |

---

## 八、关键约束与规范 — 对标文档第十二章

| 规范 | 说明 |
|------|------|
| **协作入口唯一** | 所有协作行为必须表达为 ToolCall（`call_task_agent` / `handoff_to_agent`），否则无法持久化与回放 |
| **状态写入必须走事务** | 任何对 GameWorkspace 的写入都必须通过 `WorkspaceTransaction`，否则绕过锁和 diff |
| **中断语义统一** | 取消落到 `AbortSignal`，工具执行用 `CompletableFuture` + abort 竞争 |
| **Loop 上限** | POC Agent 30 次 / Task Agent 15 次 |
| **上下文压缩阈值** | 消息数 > 100 触发 `ContextCompressionHook` |
| **工具重试上限** | `maxRetryCount` 超限进入 FIX_MODE（使用默认策略兜底） |
| **悔棋深度限制** | 快照栈最大深度可配置（默认 5 步） |

---

## 九、关键常量

```java
// RunLoop 控制
HOST_AGENT_MAX_LOOP_COUNT = 30       // 主 Agent 最大循环次数
SUB_AGENT_MAX_LOOP_COUNT = 15        // 子 Agent 最大循环次数

// 超时配置
TOOL_CALL_TIMEOUT_MS = 1_200_000     // 工具调用超时（20分钟）
WORKSPACE_LOCK_TIMEOUT_MS = 30_000   // Workspace 锁超时（30秒）

// 压缩与保护
LONG_CTX_COMPRESS_THRESHOLD = 100    // 长上下文压缩阈值（消息数）
ABNORMAL_HANDOFF_THRESHOLD = 4       // 异常 Handoff 阈值
SAME_TOOL_CALL_THRESHOLD = 5         // 相同工具调用阈值

// 落子校验
MAX_MOVE_RETRY = 3                   // 非法落子最大重试次数

// 悔棋
MAX_UNDO_DEPTH = 5                   // 最大悔棋深度（快照栈大小）
THREAT_UNDO_THRESHOLD = 0.8          // 威胁值悔棋触发阈值
```

---

## 十、实现计划 — 优先级与难度分级

### P0 — 必须实现（核心骨架）

| 任务 | 描述 | 难度 | 依赖 |
|------|------|------|------|
| **AgentRunLoop** | 实现 Agent 核心运行循环（准备上下文→调用LLM→处理响应→工具调用→循环判断） | ★★★☆☆ | 无 |
| **ToolManager** | 实现统一工具路由（call_task_agent / handoff / 内建工具） | ★★☆☆☆ | AgentRunLoop |
| **TaskAgentManager** | 实现 Task Agent 创建、运行、结果回收 | ★★★☆☆ | AgentRunLoop, ToolManager |
| **Agent 角色定义** | 定义 POC Agent 和 Task Agent 的 AgentConfig（system prompt、工具集、temperature） | ★★☆☆☆ | 无 |
| **RuleAnalysisHook + 规则分析 Task Agent** | 首回合强制解读游戏规则，输出写入 GameWorkspace 供所有 Agent 共享 | ★★☆☆☆ | HookManager, TaskAgentManager, GameWorkspace |
| **MoveValidationHook** | 落子合法性校验 + 非法落子纠正 | ★★☆☆☆ | HookManager |

### P1 — 重要功能

| 任务 | 描述 | 难度 | 依赖 |
|------|------|------|------|
| **HookManager** | 实现 Hook 注册、阶段分发、链式执行（shouldExecute → execute → continueToNextHook） | ★★★☆☆ | AgentRunLoop |
| **GameWorkspace** | 实现共享博弈状态 + 事务 + 锁 | ★★★☆☆ | 无 |
| **PocAgentManager** | 实现 POC Agent Handoff（切换配置 + 记忆摘要 + Workspace 更新） | ★★★★☆ | GameWorkspace, ToolManager |
| **GamePhaseDetectionHook** | 棋局阶段自动识别 + 阶段切换触发 Handoff | ★★☆☆☆ | HookManager, GameWorkspace |
| **LoopProtectionHook** | 重复 Handoff / ToolCall 检测 + 熔断 | ★★☆☆☆ | HookManager |
| **ContextCompressionHook** | 消息超阈值时触发上下文压缩 | ★★☆☆☆ | HookManager |

### P2 — 悔棋功能

| 任务 | 描述 | 难度 | 依赖 |
|------|------|------|------|
| **快照机制** | GameWorkspace 自动保存/回滚状态快照 | ★★★☆☆ | GameWorkspace |
| **UndoDetectionHook** | 悔棋触发条件检测（威胁分析 + 事后评估） | ★★★★☆ | HookManager, GameWorkspace |
| **UndoExecutionHook** | 悔棋状态回滚执行 | ★★★☆☆ | 快照机制 |
| **悔棋重评 Task Agent** | 悔棋后重新分析棋局并给出新走法 | ★★★☆☆ | TaskAgentManager |

### P3 — 增强优化

| 任务 | 描述 | 难度 | 依赖 |
|------|------|------|------|
| **WorkflowGuideHook** | 伪工具调用注入控制指令（forceCallTool / useTools） | ★★★☆☆ | HookManager, ToolManager |
| **ToolFilterHook** | 根据 Hook 控制指令动态过滤可用工具 | ★★☆☆☆ | WorkflowGuideHook |
| **MemorySummaryHook** | Handoff 时 LLM 生成记忆摘要 | ★★★☆☆ | PocAgentManager |
| **Trace 系统** | 结构化追踪 + Span 埋点 | ★★★☆☆ | 全部核心组件 |
| **RepeatMoveDetectionHook** | 检测重复落子建议并报警 | ★☆☆☆☆ | HookManager |
| **InputValidationHook** | 请求合法性校验 | ★☆☆☆☆ | HookManager |

---

## 十一、核心文件路径规划

```
multi-agent/src/main/java/com/gamehive/multiagent/
├── MultiAgentApplication.java              # 启动类
├── config/                                  # 配置层
│   ├── AgentConfig.java                    # Agent 角色配置
│   ├── ApiConfig.java                      # API URL 配置
│   ├── ChatClientConfig.java              # LLM 客户端配置
│   └── VectorStoreProperties.java         # 向量库配置
├── constants/                               # 常量
│   ├── AgentType.java                      # POC / TASK
│   ├── GamePhase.java                      # OPENING / MIDGAME / ENDGAME
│   ├── HookPhase.java                     # Hook 阶段枚举
│   ├── GameTypeEnum.java                  # 游戏类型
│   └── UndoPhase.java                     # 悔棋阶段枚举
├── controller/
│   └── MultiAgentController.java          # 请求入口
├── dto/                                     # 数据传输
│   ├── LMMRequestDTO.java
│   ├── LMMDecisionResult.java
│   ├── ToolResult.java                    # 工具调用结果
│   └── HookResult.java                   # Hook 执行结果
├── core/                                    # 核心引擎
│   ├── agent/
│   │   ├── BaseAgent.java                 # Agent 基类 + RunLoop
│   │   ├── PocAgent.java                  # POC Agent
│   │   └── TaskAgent.java                 # Task Agent
│   ├── loop/
│   │   └── AgentRunLoop.java              # 运行循环引擎
│   ├── tool/
│   │   ├── ToolManager.java               # 工具统一路由
│   │   ├── TaskAgentManager.java          # Task Agent 管理
│   │   └── PocAgentManager.java           # POC Agent Handoff 管理
│   ├── hook/
│   │   ├── GameHook.java                  # Hook 接口
│   │   ├── HookManager.java              # Hook 执行管理器
│   │   └── impl/
│   │       ├── InputValidationHook.java
│   │       ├── RuleAnalysisHook.java
│   │       ├── UndoDetectionHook.java
│   │       ├── UndoExecutionHook.java
│   │       ├── GamePhaseDetectionHook.java
│   │       ├── LoopProtectionHook.java
│   │       ├── RepeatMoveDetectionHook.java
│   │       ├── MoveValidationHook.java
│   │       ├── MoveRetryHook.java
│   │       ├── ContextCompressionHook.java
│   │       ├── MemorySummaryHook.java
│   │       ├── ToolFilterHook.java
│   │       ├── WorkflowPhaseTransitionHook.java
│   │       └── WorkflowGuideHook.java
│   └── workspace/
│       ├── GameWorkspace.java             # 共享博弈状态
│       ├── WorkspaceTransaction.java      # 事务 + 锁
│       ├── WorkspaceSnapshot.java         # 状态快照（悔棋用）
│       └── WorkspaceDiff.java             # 变更 diff
├── service/                                 # 服务层
│   ├── GameDecisionService.java           # 决策服务接口
│   ├── impl/
│   │   └── GameDecisionServiceImpl.java   # 决策服务实现
│   └── thread/
│       ├── Consumer.java                  # 消费线程
│       └── RequestPool.java               # 请求队列
├── trace/                                   # 可观测性
│   ├── AgentTracer.java                   # Trace 管理
│   └── TraceSpan.java                     # Span 定义
└── rag/                                     # RAG 知识库
    └── RAGChatClientFactory.java          # 按游戏类型创建 RAG 客户端
```

---

## 十二、协作模式速查

| 场景 | 使用方式 | 控制权 | 对标文档 |
|------|----------|--------|----------|
| 调用专家完成子任务 | `call_task_agent` | 保持 | §3.2 |
| 切换棋局阶段主控 | `handoff_to_agent` | 转移 | §3.3 |
| 强制调用特定工具 | Hook + `forceCallTool` | N/A | §7.4 |
| 禁用所有工具 | Hook + `useTools: "never"` | N/A | §7.5 |
| 限制可用工具 | Hook + `useTools: [...]` | N/A | §7.5 |
| 悔棋回滚 | `UndoExecutionHook` + 快照栈 | N/A | 新增 |
| 悔棋重评 | `call_task_agent("悔棋重评专家")` | 保持 | 新增 |
