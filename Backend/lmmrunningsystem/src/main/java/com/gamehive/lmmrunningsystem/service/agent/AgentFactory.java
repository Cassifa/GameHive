package com.gamehive.lmmrunningsystem.service.agent;

import com.gamehive.lmmrunningsystem.service.agent.rag.RAGChatClientFactory;
import lombok.extern.slf4j.Slf4j;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Component;

import jakarta.annotation.PostConstruct;
import java.util.ArrayList;
import java.util.List;

/**
 * Agent工厂类
 * 负责创建和管理多个GameAgent实例
 * 根据配置动态创建可用的Agent，使用Integer ID进行标识和内存隔离
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Component
@Slf4j
@ConditionalOnProperty(name = "lmm.multi-agent.enabled", havingValue = "true", matchIfMissing = true)
public class AgentFactory {

    private final ChatClient gameDecisionChatClient;
    private final ChatMemory gameChatMemory;
    private final RAGChatClientFactory ragChatClientFactory;

    // Agent配置
    @Value("${lmm.multi-agent.agents.agent1.enabled:true}")
    private boolean agent1Enabled;
    
    @Value("${lmm.multi-agent.agents.agent2.enabled:true}")
    private boolean agent2Enabled;
    
    @Value("${lmm.multi-agent.agents.agent3.enabled:true}")
    private boolean agent3Enabled;
    
    @Value("${lmm.multi-agent.agents.agent4.enabled:true}")
    private boolean agent4Enabled;
    
    @Value("${lmm.multi-agent.agents.agent5.enabled:true}")
    private boolean agent5Enabled;

    // 模型配置
    @Value("${lmm.multi-agent.agents.agent1.model:}")
    private String agent1Model;
    
    @Value("${lmm.multi-agent.agents.agent2.model:}")
    private String agent2Model;
    
    @Value("${lmm.multi-agent.agents.agent3.model:}")
    private String agent3Model;
    
    @Value("${lmm.multi-agent.agents.agent4.model:}")
    private String agent4Model;
    
    @Value("${lmm.multi-agent.agents.agent5.model:}")
    private String agent5Model;

    // 温度参数配置
    @Value("${lmm.multi-agent.agents.agent1.temperature:0.7}")
    private double agent1Temperature;
    
    @Value("${lmm.multi-agent.agents.agent2.temperature:0.7}")
    private double agent2Temperature;
    
    @Value("${lmm.multi-agent.agents.agent3.temperature:0.7}")
    private double agent3Temperature;
    
    @Value("${lmm.multi-agent.agents.agent4.temperature:0.7}")
    private double agent4Temperature;
    
    @Value("${lmm.multi-agent.agents.agent5.temperature:0.7}")
    private double agent5Temperature;

    private final List<GameAgent> availableAgents = new ArrayList<>();

    public AgentFactory(@Qualifier("gameDecisionChatClient") ChatClient gameDecisionChatClient,
                        @Qualifier("gameChatMemory") ChatMemory gameChatMemory,
                        RAGChatClientFactory ragChatClientFactory) {
        this.gameDecisionChatClient = gameDecisionChatClient;
        this.gameChatMemory = gameChatMemory;
        this.ragChatClientFactory = ragChatClientFactory;
    }

    /**
     * 初始化方法，创建所有启用的Agent
     */
    @PostConstruct
    public void initializeAgents() {
        log.info("开始初始化Agent工厂...");

        createAgentIfEnabled(1, agent1Enabled, agent1Model, agent1Temperature);
        createAgentIfEnabled(2, agent2Enabled, agent2Model, agent2Temperature);
        createAgentIfEnabled(3, agent3Enabled, agent3Model, agent3Temperature);
        createAgentIfEnabled(4, agent4Enabled, agent4Model, agent4Temperature);
        createAgentIfEnabled(5, agent5Enabled, agent5Model, agent5Temperature);

        log.info("Agent工厂初始化完成，创建了{}个可用Agent", availableAgents.size());
        
        // 记录每个Agent的信息
        for (GameAgent agent : availableAgents) {
            log.info("Agent已创建: {} (ID: {}, 温度: {})", agent.getAgentName(), agent.getId(), agent.getTemperature());
        }
    }

    /**
     * 如果启用则创建Agent
     */
    private void createAgentIfEnabled(Integer id, boolean enabled, String model, double temperature) {
        if (enabled) {
            if (model == null || model.trim().isEmpty()) {
                log.warn("Agent{} 已启用但未配置模型，使用默认ChatClient", id);
            } else {
                log.info("为 Agent{} 配置模型: {}, 温度: {}", id, model, temperature);
            }
            
            GameAgent agent = new GameAgent(
                    gameDecisionChatClient,
                    gameChatMemory,
                    ragChatClientFactory,
                    id,
                    temperature
            );
            
            availableAgents.add(agent);
            log.debug("成功创建Agent{} (ID: {}, 温度: {})", id, id, temperature);
        } else {
            log.info("Agent{} 已禁用，跳过创建", id);
        }
    }

    /**
     * 获取所有可用的Agent
     */
    public List<GameAgent> getAvailableAgents() {
        return new ArrayList<>(availableAgents);
    }

    /**
     * 获取可用Agent的数量
     */
    public int getAgentCount() {
        return availableAgents.size();
    }

    /**
     * 获取Agent的ID列表
     */
    public List<Integer> getAgentIds() {
        List<Integer> ids = new ArrayList<>();
        for (GameAgent agent : availableAgents) {
            ids.add(agent.getId());
        }
        return ids;
    }
} 