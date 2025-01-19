# GimeHive说明文档

## 1.系统概述

​	GameHive是一个基于WinForm实现的的棋类对战平台。通过多样的人工智能算法和层次分明的系统架构设计，系统能够实现与AI的对弈，同时具备多种棋类游戏模式，满足不同玩家的需求。项目通过灵活的算法设计，为用户提供多种难度的对战选择。

​	系统采用了分层架构设计，整体架构分为三个主要部分：表现层、控制层和模型层。表现层负责用户界面的展示与交互，控制层负责游戏的业务逻辑与规则执行，模型层则管理棋盘状态、玩家数据和AI算法等。分层设计的优势在于每个模块具有清晰的职责界限，降低了模块之间的耦合度，使得各个模块能够独立开发和维护，同时也便于未来的功能扩展与系统优化。

​	系统的核心优势在于其灵活的AI决策模块，支持不同的游戏规则和设置，玩家可以根据自己的需求选择游戏种类以及AI类别。主要是使用蒙特卡罗树搜索（MCTS）与博弈树（MinMax）算法实现。

## 2.系统设计

### 2.1系统架构图

![][SystemArchitectureDiagram]



### 2.2系统模块图

![][SystemModuleDiagram]

### 2.3目录文件结构

| **目录名** |                         **目录说明**                         |
| :--------: | :----------------------------------------------------------: |
| Constants  |                     存放所有全局常量枚举                     |
| Controller | 控制层，绑定所有组件与其触发事件，通过调用Model层获取系统输出，调用View层更新页面信息 |
|    View    | 视图层，接收Controller的控制，在前端更新当前信息，使用双缓冲保证动画流畅 |
|   Model    | 模型层,其中的GameManager调用实现AbstractAIStrategy的AI示例接收Controller的请求并做出相应 |
| Resources  |               存放所有图片资源，编译时一同打包               |



## 3.UI界面

### 3.1格线落子——五子棋

五子棋是15*15的棋盘，在交点落子。五子棋模式下AI可以搜索8层博弈树外加12层的算杀。右侧可以选择先后手与对弈的AI类别

![][GoBang]

### 3.2格内落子——不围棋

不围棋在格内落子，使用蒙特卡洛树在初始搜1w次下AI能展现出一定智慧，知道要走对角线，但一般还是赢不了人类

![][Anti-Go]



## 4.软件设计模式的运用

​	本系统很大的一个特色是多种设计模式的灵活组合，具体体现在Model层的各种AI管理，所有的AI都是由工厂创建的，实现了制定策略模式接口的对象，这里用到了抽象工厂模式和策略模式的融合。而为了更便于管理这些类，减少资源开销，很多类都设计成了单例模式

![][AIproducts]

### 4.1策略模式

​	在Model层的AI管理得设计中，使用了策略模式。通过定义统一的接口实现了游戏种类、运行算法得动态灵活切换，将算法从实现中分离了出来，解决了多种算法管理的问题，以下是使用策略模式相关类的类图（仅保留部分相关的方法，其余省略）

![][StrategyInterface]

**1.**    **定义抽象类，统一接口**

​	在Model模块中，AbstractAIStrategy 抽象类充当了所有AI策略的统一接口，其核心职责是明确AI算法必须完成的功能。例如，GetNextAIMove 用于获取AI的下一步决策，CheckGameOverByPiece 判断游戏是否结束，GameStart 和 GameForcedEnd 管理AI的生命周期。

​	这种设计确保了所有AI算法的功能入口和行为一致，使得棋盘管理类 BoardManager 能够以相同的方式调用不同算法，而无需关心具体实现细节。这不仅提高了代码的可读性，也避免了不同算法间的耦合。

**2.**    **支持多种AI****算法的实现**

​	不同AI算法可以通过继承 AbstractAIStrategy 实现自己的逻辑。例如，负极大值搜索算法 Negamax 继承了这一抽象类，并提供了自己的实现细节。Negamax 算法的设计中包含了博弈树的递归搜索逻辑，以及对游戏局面的胜负评估。这种实现方式使得 Negamax 能够与其他算法（如 MCTS ）共存，且相互独立。

​	每种算法只需要专注于实现自身的核心逻辑，而无需修改或依赖其他模块的代码。这种设计方式减少了代码的复杂性，同时降低了因改动某一算法而导致其他部分出问题的风险。

**3.**    **动态切换AI****策略**

​	棋盘管理类 BoardManager 提供了一个动态切换AI策略的功能。通过方法 SwitchAIType，系统可以根据用户的选择指定运行时的AI算法。例如，当用户在界面上选择不同AI策略时，BoardManager 会调用对应工厂方法生成AI实例并切换到选定的策略。

​	这种动态切换机制使得项目能够快速适配新需求，尤其是在测试和比赛环境中可以尝试多种AI策略，选出最优解法。重要的是，这种切换完全依赖于抽象接口的设计，不需要对现有实现进行任何改动，充分体现了策略模式的灵活性。

**4.**    **解耦棋盘管理和算法实现**

​	棋盘管理类 BoardManager 与AI策略实现完全解耦，这一点是策略模式设计的关键。具体来说，BoardManager 只负责调用 AbstractAIStrategy 提供的接口获取AI的下一步棋、检查游戏状态，而无需了解背后是负极大值搜索还是其他算法。

​	比如，在 LetAIMove 方法中，BoardManager 调用 GetNextAIMove 来获取AI的下一步行动，这一步操作的逻辑并不依赖具体算法的实现。无论算法多复杂，它们的细节都被隐藏在接口实现中。这样一来，BoardManager 的代码得以保持简洁，同时不同算法的复杂性也不会相互干扰。

**5.**    **扩展性和维护方便**

​	策略模式的应用使得系统具有高度的扩展性。例如，如果未来需要新增一种AI算法，只需要继承 AbstractAIStrategy 并实现相关方法即可，BoardManager 和其他模块的代码无需任何修改。

​	这一点在面对新需求时尤为重要。比如，当引入更复杂的棋类游戏规则或更智能的AI策略时，新算法可以直接插入到系统中而不会影响已有的代码逻辑。同时，由于各算法之间互不依赖，调试和维护时可以聚焦于单一算法，大大减少了问题排查的难度。

### 4.2抽象工厂模式

**1.** **定义**

​	抽象工厂模式（Abstract Factory Pattern），它围绕一个超级工厂创建其他工厂。该超级工厂又称为其他工厂的工厂。它也是一种创建型设计模式，提供了一种创建对象的最佳方式。

![][AbstractFactory]

**2.** **特点**

在抽象工厂模式中，接口是负责创建一个相关对象的工厂，不需要显式指定它们的类。每个生成的工厂都能按照工厂模式提供对象。

抽象工厂模式提供了一种创建一系列相关或相互依赖对象的接口，而无需指定具体实现类。通过使用抽象工厂模式，可以将客户端与具体产品的创建过程解耦，使得客户端可以通过工厂接口来创建一族产品。

**3.** **抽象工厂优点**

客户端独立于具体的实现类：客户端使用抽象工厂来创建产品，而不需要关心具体的实现类。这样可以降低客户端与具体实现类之间的耦合度。

易于切换产品系列：由于客户端只使用抽象工厂来创建产品，所以只需要切换具体的工厂实现类，就可以切换到不同的产品系列。

确保产品组合的一致性：每个具体工厂负责创建一个产品系列，这确保了产品之间的一致性。

支持产品等级结构：抽象工厂模式提供了一种支持产品等级结构的方式，可以很容易地增加新的产品。

- **抽象工厂类**

​	在系统中，我通过抽象工厂模式实现了对不同游戏AI类型的统一管理与实例化。例如，通过定义一个抽象工厂 AbstractFactory，规定了需要提供的四种AI算法（MCTS、MinMaxMCTS、MinMax 和 Negamax），以及相关的棋盘信息 GameBoardInfo。

```java
/*************************************************************************************
 * 文 件 名:   AbstractFactory.cs
 * 描    述: 抽象工厂，规定了四种类型的AI
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:05
*************************************************************************************/
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory {
    internal abstract class AbstractFactory {
        protected GameBoardInfo boardInfo;
        public abstract MinMaxMCTS GetMinMaxMCTSProduct();
        public abstract MCTS GetMCTSProduct();
        public abstract MinMax GetMinMaxProduct();
        public abstract Negamax GetNegamaxProduct();
        public GameBoardInfo GetBoardInfoProduct() {
            return boardInfo;
        }
    }
}

```

- **切换游戏与算法**

​	抽象工厂模式的引入大幅提升了系统的扩展性和可维护性。通过调用 SwitchGame 方法，系统可以根据游戏类型动态切换不同的工厂实例，并生成对应的棋盘信息和AI产品实例。而通过 SwitchAIType 方法，可以进一步切换具体的AI算法类型，从而实现更灵活的AI策略组合。

```java
//切换算法
public GameBoardInfo SwitchGame(GameType gameType) {
    this.gameType = gameType;
    // 根据当前游戏类型选择对应的工厂
    factory = gameType switch {
        GameType.Gobang88 => gobang88Factory ??= Gobang88Factory.Instance,
        GameType.Gobang => gobangFactory ??= GobangFactory.Instance,
        GameType.MisereTicTacToe => misere ??= MisereTicTacToeFactory.Instance,
        GameType.AntiGo => antiGoFactory ??= AntiGoFactory.Instance,
        GameType.TicTacToe => ticTacToeFactory ??= TicTacToeFactory.Instance,
        _ => throw new NotSupportedException($"Unsupported game type: {gameType}")
    };
    //更新为新游戏的BoardInfo并返回
    this.BoardInfo = factory.GetBoardInfoProduct();
    return BoardInfo;
}
//切换游戏类型
public void SwitchAIType(AIAlgorithmType type) {
    this.aIAlgorithmType = type;
    // 根据 AI 类型从工厂获取对应的实例
    runningAI = type switch {
        AIAlgorithmType.MinMaxMCTS => factory.GetMinMaxMCTSProduct(),
        AIAlgorithmType.MCTS => factory.GetMCTSProduct(),
        AIAlgorithmType.AlphaBetaPruning => factory.GetMinMaxProduct(),
        AIAlgorithmType.Negamax => factory.GetNegamaxProduct(),
        _ => throw new NotSupportedException($"不支持此算法类型: {type}")
    };
}
```

- **具体工厂实例**

​	以代码为例，具体工厂如 AntiGoFactory 实现了 AbstractFactory，针对反围棋（AntiGo）游戏提供了一系列AI产品实例。这些AI产品包括 AntiGoMCTS 和 AntiGoMinMaxMCTS，它们分别实现了MCTS和MinMaxMCTS策略，同时也明确指定了不支持的算法（如Negamax），通过抛出异常的方式来表明此工厂对这些产品不提供支持。

​	此外，为了确保工厂的唯一性和线程安全性，系统对具体工厂的实例化采用了单例模式。例如，AntiGoFactory 通过 Instance 属性确保在多线程环境下安全地返回唯一的实例。提高了系统的资源利用效率，确保了工厂的行为一致性。

```java
/*************************************************************************************
 * 文 件 名:   ReversiFactory.cs
 * 描    述: 不围棋工厂
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:36
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.AIFactory.ConcreteProduct.AntiGo;

namespace GameHive.Model.AIFactory {
    internal class AntiGoFactory : AbstractFactory {
        public override MCTS GetMCTSProduct() {
            return new AntiGoMCTS();
        }

        public override MinMaxMCTS GetMinMaxMCTSProduct() {
            return new AntiGoMinMaxMCTS();
        }

        /*——————————不可用———————————*/

        public override Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }
        public override MinMax GetMinMaxProduct() {
            return new AntiGoMinMax();
        }

        //单例模式
        private static AntiGoFactory _instance;
        private AntiGoFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.MCTS,
                AIAlgorithmType.MinMaxMCTS,
            };
            boardInfo = new GameBoardInfo(7, true, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static AntiGoFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(AntiGoFactory)) {
                        if (_instance == null) {
                            _instance = new AntiGoFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}

```

​	在此系统中抽象工厂模式的运用带来了很多好处，解耦是其中最重要的好处之一。客户端并不需要直接依赖具体的工厂和产品，而是通过抽象工厂接口来创建所需的对象。例如，在不同的棋类游戏（如围棋、反围棋）中，我们只需要选择相应的工厂（如 AntiGoFactory 或 GobangFactory），客户端不需要关心具体的AI算法实现，抽象工厂会根据游戏类型和AI算法类型自动创建对应的AI实例，而此实例由是实现了策略模式接口的产品。这种让系统代码与具体实现解耦，方便未来扩展和维护。

​	易于扩展是另一个优势。假设要在系统中增加一个新的棋类游戏，我只需要创建一个新的工厂类来实现 AbstractFactory 接口，并在其中定义新的AI算法。现有的代码不需要做任何改动，客户端只需通过选择新的工厂类来使用新产品。这种扩展方式不仅简便，而且不会影响现有的功能，减少了开发和维护的复杂度。

​	一致性保证也体现在产品的配合上。在每个具体工厂中，所有生成的产品都是属于同一系列的，保证了它们的协调性。例如，AntiGoFactory 生成的产品包括反围棋算法和反围棋的棋盘信息，它们是为反围棋游戏设计的，确保了AI算法与游戏规则的一致性。客户端不必担心不匹配的产品被混合使用，从而减少了出错的概率。

​	单例模式的结合使得工厂的管理变得更加高效。每个具体工厂（如 AntiGoFactory）都通过单例模式保证只有一个实例，避免了重复创建工厂的资源浪费，也确保了工厂实例的统一性。对于内存管理和性能来说，使用单例模式能减少不必要的开销，提升系统的稳定性和效率。抽象工厂模式在我们系统中的运用，不仅让代码更加清晰和易于扩展，还能确保不同游戏逻辑和AI策略的配合更加一致和稳定。

### 4.3单例模式

​	本系统中，单例模式主要运用于在非常多的地方。系统分的三层Controller、View、Model。其中Controller、View、和Model中的GameManager涉及的所有类全部是partial class，即本质上它们都只有一个类。而这些类全部为单例模式，因为在整个游戏运行过程中只需要一个实例即可；此外所有的具体工厂也是单例模式的

​	工厂类的实例化管理，确保在整个游戏过程中，每种棋类的工厂实例只有一个。这样做不仅避免了重复创建工厂实例的开销，还能保证工厂的全局唯一性，使得系统更加高效、稳定。

​	一个典型的例子是 AntiGoFactory，这是专门为反围棋游戏提供AI算法实例和棋盘信息的工厂。该工厂类采用单例模式确保在系统中只存在一个 AntiGoFactory 实例。通过单例模式，整个系统能够统一管理工厂实例，避免了多次实例化带来的资源浪费。



## 5.博弈树实现及优化

#### 5.1博弈树概念

​	在博弈过程中, 任何一方都希望自己取得胜利。因此，当某一方当前有多个行动方案可供选择时, 他总是挑选对自己最为有利而对对方最为不利的那个行动方案。 此时,如果我们站在A方的立场上,则可供A方选择的若干行动方案之间是“或”关系, 因为主动权操在A方手里,他或者选择这个行动方案, 或者选择另一个行动方案, 完全由A方自己决定。当A方选取任一方案走了一步后,B方也有若干个可供选择的行动方案, 此时这些行动方案对A方来说它们之间则是“与”关系,因为这时主动权操在B方手里,这些可供选择的行动方案中的任何一个都可能被B方选中, A方必须应付每一种情况的发生。

这样,如果站在某一方(如A方,即在A要取胜的意义下), 把上述博弈过程用图表示出来, 则得到的是一棵“与或树”。 描述博弈过程的与或树称为博弈树,它有如下特点： 

1. 博弈的初始格局是初始节点。 
2. 在博弈树中, “或”节点和“与”节点是逐层交替出现的。**自己一方扩展的节点之间是“或”关系, 对方扩展的节点之间是“与”关系。双方轮流地扩展节点。** 
3. 所有自己一方获胜的终局都是本原问题, 相应的节点是可解节点；所有使对方获胜的终局都是不可解节点。

#### 5.2极大值极小值分析法

在二人博弈问题中,为了从众多可供选择的行动方案中选出一个对自己最为有利的行动方案, 就需要对当前的情况以及将要发生的情况进行分析,从中选出最优的走步。最常使用的分析方法是极小极大值分析法。 其基本思想是： 

1. 设博弈的双方中一方为A,另一方为B。然后为其中的一方(A)寻找一个最优行动方案。
2. 为了找到当前的最优行动方案, 需要对各个可能的方案所产生的后果进行比较。具体地说, 就是要考虑每一方案实施后对方可能采取的所有行动, 并计算可能的得分。
3. 为计算得分,需要根据问题的特性信息定义一个估价函数, 用来估算当前博弈树端节点的得分。此时估算出来的得分称为静态估值。 
4. 当端节点的估值计算出来后, 再推算出父节点的得分, 推算的方法是：**对“或”节点**, **选其子节点中一个最大的得分作为父节点的得分,**这是为了使自己在可供选择的方案中选一个对自己最有利的方案；对“与”节点, **选其子节点中一个最小的得分作为父节点的得分,**这是为了立足于最坏的情况。这样计算出的父节点的得分称为倒推值。 
5.  如果一个行动方案能获得较大的倒推值, 则它就是当前最好的行动方案。

![][MinMaxSearch]

#### 5.3博弈树抽象产品类

​	MinMax抽象类实现策略接口AbstractAIStrategy，并作为抽象工厂的抽象产品类，定义实现一系列博弈树方法。派生出GoBangMinMax、GoBang88MinMax、MisereTicTacToeMinMax、TicTacToeMinMax四种游戏对应的产品。MinMax抽象类负责实现EvalToGo函数，次函数进行极大值极小值搜索，定义GetAvailableMoves、GetAvailableMovesByNewPieces、PlayChess、GetCurrentBoard、EvalNowSituation五个抽象函数，子类通过重写这五个函数可实现添加一种博弈树具体产品的效果

![][MinMaxProductClassDiagram]



## 6.博弈树的优化

### 6.1α-β剪枝

​	极小极大分析法, 实际是先生成一棵博弈树,然后再计算其倒推值。这样做的缺点是效率较低。于是,人们又在极小极大分析法的基础上,提出了α-β剪枝技术。

​	这一技术的基本思想是,边生成博弈树边计算评估各节点的倒推值, 并且根据评估出的倒推值范围,及时停止扩展那些已无必要再扩展的子节点, 即相当于剪去了博弈树上的一些分枝, 从而节约了机器开销, 提高了搜索效率。具体的剪枝方法如下: 

   

1. 对于一个与节点MIN, 若能估计出其倒推值的上确界β,并且这个β值不大于MIN的父节点(一定是或节点)的估计倒推值的下确界α,即α≥β, 则就不必再扩展该MIN节点的其余子节点了(因为这些节点的估值对MIN父节点的倒推值已无任何影响了)。这一过程称为α剪枝。
2. 对于一个或节点MAX, 若能估计出其倒推值的下确界α, 并且这个α值不小于MAX的父节点(一定是与节点)的估计倒推值的上确界β, 即α≥β,则就不必再扩展该MAX节点的其余子节点了(因为这些节点的估值对MAX父节点的倒推值已无任何影响了)。 这一过程称为β剪枝。

​	Max 分支挑选出来的最高分记为 α；Min 分支挑选出来的最低分记为 β。初始时， α 值为无穷小 -∞，β 值为无穷大 +∞。

​	Min 分支返回给 Max 分支的最小分值其实就是当前 Max 分支的分值，所以需要更新的是 α 值，同样的，Max 分支会选择最大的一个 α 值返回给 Min 分支，这个 α 值也就是当前 Min 分支的分值，所以需要更新的是 β 值。**α** **值的来源是 β，β 值的来源是 α**（除了叶子结点，因为叶子结点的值来源于评估函数）

![][Pruning]

### 6.2启发式搜索

​	启发式搜索(Heuristically Search)又称为有信息搜索(Informed Search)，它是利用问题拥有的启发信息来引导搜索，达到减少搜索范围、降低问题复杂度的目的，这种利用启发信息的搜索过程称为启发式搜索。

#### 6.1 限制搜索宽度

​	在博弈树每层拓展节点的过程中是对棋盘内的所有位置都进行了拓展并搜索。但是不难发现，有些点位好像根本就不用加入到搜索列表，如：

- 下棋后立刻导致失败的节点

​	如果某一落子直接导致了己方失败或者被对方吃掉了重要棋子，可以立即将该节点从搜索列表中移除。例如，在象棋或围棋等棋类游戏中，若当前玩家的落子会导致被对方将死、吃掉关键棋子，或直接违反游戏规则（如过早出现棋局终结状态），这类节点不可能成为最终的选择，因此可以被排除。

- 离当前其它棋子过远的棋子

​	在五子棋游戏中，棋盘上某些位置的棋子可能离其他棋子距离过远，无法参与当前的博弈或者未来的局部变化。比如在围棋中，如果某个棋子孤立在棋盘的某个角落且无任何连接，且没有可能成为关键棋子，那么这个节点的拓展可以忽略不计。在国际象棋中，若某个棋子与对方的棋子距离遥远且无实际威胁，也可以排除该节点。

​	这些节点必然不可能导致一个收益较高的局面，不会被选为最终决策，因此可以直接略过。

#### 6.2 优化搜索顺序

​	在Alpha-Beta剪枝中，优化搜索顺序是一个关键环节。它通过改变节点扩展的顺序来提高剪枝的效果。在没有优化顺序的情况下，Alpha-Beta剪枝的效率往往低于预期，因为如果最先扩展的节点并未很好地“剪掉”其他不必要的节点，搜索过程会仍然涉及大量无效的计算。

​	在Max层迅速的搜到估值较高的点可以快递的更新Beta值，以调高Beta剪枝率

​	在Min层迅速的搜到估值较低的点可以快递的更新Alpha值，以调高Alpha剪枝率

​	即，在任意一方下棋时应该尽可能优先去评估对其有利的局面，以迅速缩小Apha-Beta窗口

​	因此，启发式搜索可以在一定程度上改善搜索顺序，从而更快地达到剪枝效果。通过根据某些启发式信息，如当前局势的优劣、对手的可能策略等，优先搜索那些可能更具威胁或更有利的节点。这样可以在最早的阶段就减少大量无关节点的计算，从而显著提升搜索效率。常见的启发式排序方法包括：

- 最有可能成功的行动优先：根据棋局评估函数，优先扩展评分较高的动作节点。
- 对抗性排序：在对弈的博弈树中，优先搜索对方可能对自己造成威胁的节点，从而确保能尽早避免潜在的失败。

#### 6.3 引入评估函数

​	评估函数是评估棋局好坏的标准，用来指导搜索过程中的节点扩展。通过使用评估函数，搜索算法可以在没有完整展开所有节点的情况下，快速评估当前局面的优劣。这对于深度搜索尤其重要，因为搜索深度越大，计算量越大，若没有启发式函数的引导，往往会导致计算无法完成或效率低下。

​	启发式评估函数通常基于棋局的局部信息，如棋盘上的棋子分布、双方棋子与关键位置的距离、可能的未来移动等。评估函数的设计应当能够尽可能准确地反映局面的优劣，并且能够与搜索策略有效结合。

**启发函数：**

```java
//关键函数✨将底数限制为8,不超过12 对可行点进行排序：在AI-Max层有限搜估值高或危险的点位，Player-Min层搜人类增益最高的点
//AI要防人杀棋同时贪心的搜当前价值最高点，人要贪心的走价值增益最大的点 由于运行MinMax前已经计算过AI杀棋，所以不考虑AI杀情况
//如果是AI-将落子点位对AI增益从大到小，并加入人类VCT杀棋点,预防人类连续冲三攻击。若存在有价值点则不考虑无价值点
//如果是人类 落子点增益最大的10个点(考虑了AI的增益)
protected override void HeuristicSort(ref List<Tuple<int, int>> moves, int lastX, int lastY) {
    bool isAI = (lastX == -1 || NormalBoard[lastX][lastY] == Role.AI) ? false : true;
    if (isAI) { //将可行点位按增益从大到小排序，前置人类VCT杀棋点
        //暂存队列，item1为点位，item2为对于AI估值增益和玩家杀棋价值
        var scoredMoves = new List<Tuple<Tuple<int, int>, Tuple<int, KillingBoard>>>();
        foreach (var move in moves)
            scoredMoves.Add(Tuple.Create(move, Tuple.Create(GetAIPointDeltaScore(move.Item1, move.Item2),
                EvaluateKill(move, Role.Player))));
        moves.Clear();
        scoredMoves.Sort((a, b) => {
            int scoreA = a.Item2.Item1;
            int scoreB = b.Item2.Item1;
            // 如果杀棋价值不同，优先考虑杀棋->人类有潜力VCT的全部前置,防止被VCT
            int killA = a.Item2.Item2.score;
            int killB = b.Item2.Item2.score;
            if (killA != killB)
                return killB.CompareTo(killA);
            return scoreB.CompareTo(scoreA);
        });
        //如果存在有分数的且人类无法形成VCT那么不考虑没分数的，若有分数点已经8个或者VCT＋有分数已经15个，拒绝价值更低点位
        int hasScore = 0;
        for (int i = 0; i < scoredMoves.Count; i++) {
            var t = scoredMoves[i];
            if (moves.Count != 0 && t.Item2.Item1 <= 0 && t.Item2.Item2.score <= 0) break;
            if (t.Item2.Item1 != 0) hasScore++;
            moves.Add(t.Item1);
            if (hasScore > 8 || i > 12) break;
        }
        if (moves.Count == 0)
            return;
    } else {
        //人类在下棋，优先搜可下棋后人类增益最大的点（考虑了阻碍AI增益）
        var scoredMoves = new List<Tuple<Tuple<int, int>, int>>();
        foreach (var move in moves)
            scoredMoves.Add(Tuple.Create(move, GetPlayerPointDeltaScore(move.Item1, move.Item2)));
        moves.Clear();
        //将杀棋点前置并将可行点位按AI增益估值从小到大排序，不考虑AI估值大于KillTypeEnum.Low
        scoredMoves.Sort((a, b) => {
            //杀棋值相同，按估值从小到大排序
            return b.Item2.CompareTo(a.Item2);
        });
        //如果已经有大量有增益的，不考虑毫无增益的
        //如果大于10个有增益的直接终止
        foreach (var t in scoredMoves) {
            if ((moves.Count > 5 && t.Item2 == 0) || moves.Count >= 8) break;
            moves.Add(t.Item1);
        }
        if (moves.Count == 0)
            return;
    }
}
```

### 6.3AC自动机

​	在五子棋AI的设计中，AC自动机被用来加速模式匹配的过程，从而提升局面估值的效率。五子棋的局面估值主要依赖于查找棋盘中是否出现了特定的棋局模式，例如“活三”、“冲四”等。传统的匹配方法需要逐行、逐列、逐对角线地遍历棋盘，并且对每一个位置进行模式匹配，计算代价较高。

​	为了解决这个问题，AI使用了 AC自动机 来加速模式匹配。通过将所有可能的局面模式（如“活三”或“冲四”）插入到AC自动机中，AI可以在遍历棋盘时同时匹配多个模式，而不必每次都从头开始匹配。

![][ACAutomationBuild]

​	在实际使用中，AC自动机的 CalculateLineValue 方法用于计算每一行、每一列和每一条对角线的模式匹配得分。具体来说，当遍历棋盘的某一行时，AC自动机会快速计算该行中出现的所有模式，并将其对应的得分累加到局面估值中。使用AC自动机进行模式匹配，能够显著提高匹配速度，减少计算复杂度。具体来说，对于一个n*n棋盘使用暴力估值整个棋盘需要4*n*n*m*cnt的时间，其中m为模板串平均大小，cnt为模板串数量，使用AC自动机优化后只需要4*n*n的时间，相当于优化掉了模式串总长度的时间。而五子棋有40余个模板串，平局长度约为5，相当于AC自动机将估值过程加速了**200倍**。

### 6.4Zobrist缓存优化

​	对于很多棋类游戏，操作步骤不同而导致相同局面时局面的估值依然是一样的。即：局面估值与路径无关

​	在极大极小值搜索的过程中，其实存在很多这种落子顺序不同，但局面相同的情况。如果我们能够将这些局面评分缓存起来，等到下次遇到相同的局面那就不用每次都进行分数评估了，可以节省不少时间。

​	Zobrist哈希是一种广泛应用于棋类游戏中的优化技术，通过高效地计算棋盘的哈希值来加速局面评估和搜索过程。

​	Zobrist哈希的核心思想是为每个棋盘上的每个位置分配一个唯一的随机哈希值。对于每个棋子（如玩家A、玩家B或空白），都会为其分配一个唯一的哈希值。

### 6.5迭代加深

​	有时候会发现AI在必应局面时会“调戏”玩家，表现为AI会“故意”不在马上胜利的点位落子，而是先走其它字。这是因为在一定搜索深度限制下可能同时存在多个必赢得局面，导致其会随机选择一个局面，而如果没有选择到马上胜利点位则直观感受就是AI在“嘲讽”玩家。

​	下图中可以发现其实在第二层就已经搜索到了答案，但是如果不加限制，搜索树会一直搜到深层，最后才回到第二层。

![][IterativeDeepening]

​	因此我们会发现，有时候其实并不需要搜索到6层或者是8层后再去结束搜索过程，最优解可能会在第4层，甚至是第2层就可以找到。那我们怎样才可以知道我们要找的最优解是在第几层呢？

​	此时可以利用 迭代加深搜索 算法。迭代深化搜索（iterative deepening search）或者更确切地说迭代深化深度优先搜索 (iterative deepening depth-first search (IDS or IDDFS)) 是一个状态空间（状态图）搜索策略。在这个搜索策略中，一个具有深度限制的深度优先搜索算法会不断重复地运行，并且同时放宽对于搜索深度的限制，直到找到目标状态。

​	先确定我们要搜索的最大深度 maxDepth，然后再确定第一次搜索的深度 depth，以及下一次搜索的深度增量 offset，直到找到最优解后，终止整个搜索过程。

​	假设我们最大需要搜索 6 层，那我们可以先尝试一下搜索 2 层，如果没有搜索到最优解，我们就再在之前搜索的深度上加上 2 层，也就是再尝试搜索 4 层，只要不超过我们设置的最大搜索深度就继续搜索，如果这个时候搜索到了最优解，我们就直接返回这个结果，结束整个搜索过程。

### 6.6负极大值博弈树

​	在博弈树中，对于敌方而言，取得是估值最小节点。但是如果也让敌方取最大，只是最后加一个负号，那么不也相当于取最小吗？而且这样会简化我们的代码，使得我们的代码变得更少更优雅。于是就产生了负值极大法。

​	负值极大法（Negamax）是一种基于极小化搜索树的博弈算法。它基于一个基本的观察：对于一个玩家而言，最好的策略与对手的最差策略是等价的。因此，通过将对手的收益取负值，问题可以转化为一个极大化搜索问题。

​	这两种算法本质上是相似的，只是表达方式不同。在Negamax算法中，通过将对手的收益取负值，将问题转化为一个极大化搜索问题；而在Minimax算法中，通过交替最大化和最小化的方式，在博弈树中搜索最佳决策。

**负极大值代码：**

```java
private int EvalToGo(List<List<Role>> currentBoard, int depth) {
    Role currentPlayer = (depth & 1) == 1 ? Role.AI : Role.Player;
    // 检查当前局面的胜负情况
    Role winner = CheckGameOverByPiece(currentBoard, -1, -1);
    if (winner == Role.Draw) return 0;
    if (winner == currentPlayer) return 1_000_000;
    if (winner != Role.Empty) return -1_000_000;

    // 初始化最大分数
    int maxScore = int.MinValue;
    Tuple<int, int>? bestMove = null;

    var availableMoves = GetAvailableMoves(currentBoard);
    foreach (var move in availableMoves) {
        currentBoard[move.Item1][move.Item2] = currentPlayer;
        int score = -EvalToGo(currentBoard, depth + 1);
        if (score > maxScore || (score == maxScore && new Random().Next(2) == 0)) {
            maxScore = score;
            bestMove = move;
        }
        currentBoard[move.Item1][move.Item2] = Role.Empty;
    }
    if (bestMove != null) FinalDecide = bestMove;
    return maxScore;
}
```



## 7.五子棋算杀

### 7.1算杀介绍

​	算杀就是计算出自己稳赢的一条落子路径。五子棋中的算杀模块主要用于判断当前局面下，是否存在可以直接制胜的走法。算杀的目标是通过判断局面来识别对方可能的死棋或威胁，并采取行动予以应对或利用这些机会来制胜。算杀模块的实现可以极大地提高AI的决策效率和对局面的响应能力。

### 7.2VCF

Victory of Continuous Four，指利用连续不断冲四这种绝对先手，直至最终成五而取得胜利的一种技巧。简称“连续冲四胜”

![][VCF1]

如上图所示，当前局面，白子已经形成两个活三了，照一般棋手来说，此时的黑子已经难以抵挡的住了，白子马上就能赢得胜利。

如果此时的黑子是个高手，那么他马上就可以想到，利用 VCF 反夺此局的胜利。

VCF就是一直找自己能够 冲四 的点位，然后尝试落子到该点位，一直这么尝试就行，尝试之后，看下自己的棋子是不是形成了 冲四活三 或者别的能赢的棋型，能形成就证明 VCF 成功了。

上图的局面找一找黑子可以 冲四 的点位，可以发现坐标 (10,4) 可以形成 冲四。

![][VCF2]

黑子 冲四 了，那白子下一步肯定得走 (10,3) 去拦截黑子，此时不拦的话白子就输了，所以此子是必会走的。

![][VCF3]

黑子继续找可以 冲四 的点位，发现坐标 (11,5) 可以，然后落子到该处。

![][VCF4]

此时黑子的棋型不只是有 冲四，还形成了一个 活三 ，白子下一步肯定要先去拦截掉黑子的 冲四 的，所以黑子在其拦截之后，马上落子到 (9,3) 或 (13,7) 就能形成 活四 了，而此时的白子不管怎么下，都阻挡不了黑子获胜。

![][VCF5]

最终黑子活四，胜利

### 7.3VCT

Victory of Continuous Three，指利用连续不断地活三、冲四、做VCF三种攻击手段，最终获胜的战术技巧。

此局的黑子尝试使用 VCT 就能发现自己其实有一条稳赢的落子路径。

![][VCT1]

VCT 是找 冲四、活三 等多种点位，最后也是看落子能不能形成 冲四活三 或者别的能赢的棋型，能形成就证明 VCT 成功了。

黑子有8个点位可以形成 活三，没发现可以形成 冲四 的点位。

![][VCT2]

然后让黑子依次尝试落子到那8个点位中，分别能够形成8种不同的 活三 ，既然黑子都形成活三了，那下一步的白子肯定会去拦截黑子的 活三，以防止黑子形成 活四 ，这样黑子一直尝试落子形成 活三 ，白子去拦截黑子，到最后我们就能发现，黑子落子到坐标 (9,9) 可以必胜。

黑子落子到 (9,9) 形成 活三

![][VCT3]

白子必须阻挡黑子形成 活四，所以白子落子到 (10,9)

![][VCT4]

黑子此时发现坐标 (9,10) 也可以形成 活三，而且还是两个 活三 ，所以落子到此处必胜了

![][VCT5]

白字不能通过VCF反击，所以黑子已经必胜

## 8.蒙特卡洛搜索树实现及优化

### 8.1蒙特卡洛介绍

蒙特卡洛树搜索是一种基于树结构的蒙特卡洛方法，所谓的蒙特卡洛树搜索就是基于蒙特卡洛方法在整个2N（N等于决策次数，即树深度）空间中进行启发式搜索，基于一定的反馈寻找出最优的树结构路径（可行解）。概括来说就是，MCTS是一种**确定规则驱动的启发式随机搜索算法。**

1. 树结构：树结构定义了一个可行解的解空间，每一个叶子节点到根节点的路径都对应了一个解（solution），解空间的大小为2N（N等于决策次数，即树深度）
2.  蒙特卡洛方法：MSTC不需要事先给定打标样本，随机统计方法充当了驱动力的作用，通过随机统计实验获取观测结果。
3. 损失评估函数：有一个根据一个确定的规则设计的可量化的损失函数（目标驱动的损失函数），它提供一个可量化的确定性反馈，用于评估解的优劣。从某种角度来说，MCTS是通过随机模拟寻找损失函数代表的背后”真实函数“。
4. 反向传播线性优化：每次获得一条路径的损失结果后，采用反向传播（Backpropagation）对整条路径上的所有节点进行整体优化，优化过程连续可微
5. 启发式搜索策略：算法遵循损失最小化的原则在整个搜索空间上进行启发式搜索，直到找到一组最优解或者提前终止

**蒙特卡洛搜索步骤：**

![][MCTSFlow]

算法的优化核心思想总结一句话就是：在**确定方向的渐进收敛（树搜索的准确性）和随机性（随机模拟的一般性）之间寻求一个最佳平衡**。体现了纳什均衡的思想精髓。

**蒙特卡洛树构建：**

![][MCTSBuild]

### 8.2蒙特卡洛类设计

​	MCTS抽象类为AI工厂生产的一类抽象产品，实现AbstractAIStrategy策略供GameManager控制，派生了不围棋、井字棋、反井字棋三个具体产品。MCTS抽象类主要负责实现蒙特卡洛运行流程控制、实现AbstractAIStrategy方法、MCTS多线程搜索三项职责。并定义CheckGameOverByPiece和GetAvailableMoves抽象方法共子类重载。具体产品只需实现这两个函数即可实现一种棋类游戏的蒙特卡洛产品实例。

​	MCTS负责实现了蒙特卡洛算法中的：选择、模拟、扩展三个步骤。反向传播过程交给MCTSNode类代理完成

**蒙特卡洛抽象类工厂：**

![][MCTSClassDiagram]

**蒙特卡洛节点类：**

![][MCTSNodeClassDiagram]

### 8.3多线程搜索优化

​	MCTS类内部定义一个根节点和互斥锁，当游戏开始后就会初始化节点并启动搜索线程开始搜索，在玩家思考时搜索线程会在后台搜索。当需要AI给出决策时会在搜索一定次数后返回当前根节点的决策。

​	当收到玩家下棋时GetAIMoves抽象策略会竞争互斥锁并根据玩家移动切换根节点至当前局面对应节点。并清除搜索次数，释放锁直至达到搜索次数后再次竞争锁并返回AI决策。

​	当游戏开始或结束时会重置游戏运行状态，并杀死或再次启动搜索线程

```java
public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
    //争夺锁，换根
    lock (mutex) {
        Role winner;
        //AI先手会根据(-1,-1)构造初始棋盘
        if (lastX == -1) winner = Role.Empty;
        else winner = CheckGameOverByPiece(currentBoard, lastX, lastY);
        //根据玩家决策换根
        if (lastX != -1) {
            PlayedPiecesCnt++;
            RootNode = RootNode.MoveRoot(lastX, lastY, NodeExpansion);//传入函数指针
        }
        AIMoveSearchCount = 0;

        //释放锁并等待搜索线程通知-收到通知后判断是否达标
        while (AIMoveSearchCount < SearchCount)
            Monitor.Wait(mutex);
        //根据AI决策换根
        Tuple<int, int> AIDecision = RootNode.GetGreatestUCB().PieceSelectedCompareToFather;
        RootNode = RootNode.MoveRoot(AIDecision.Item1, AIDecision.Item2, NodeExpansion);
        PlayedPiecesCnt++;
        if (NeedUpdateSearchCount)
            UpdateSearchCount(baseCount, TotalPiecesCnt * TotalPiecesCnt, PlayedPiecesCnt, ref SearchCount);
        return AIDecision;
    }
}
```

​	在搜索线程中每次会执行一定次数的搜索再释放一次锁，让GetAIMoves函数有机会竞争锁。若释放后依然GetAIMoves没有争夺锁则会继续执行搜索。直至锁被竞争或游戏结束后停止

```java
//执行一次搜索任务
private void EvalToGo() {
    //一直执行直到结束
    while (!end) {
        lock (mutex) {
            //搜最小单元后释放一次锁
            for (int i = 0; i < MinSearchCount; i++)
                SimulationOnce();
            AIMoveSearchCount += MinSearchCount;
            Monitor.Pulse(mutex);
        }
    }
}
```

多线程搜索使得AI利用上了玩家决策的时间并且能够利用历史的搜索记录，大大提升了AI的性能



## 9.系统优点

### 9.1 职责分层

​	系统采用了明确的分层架构，从功能上划分了多个层次，包括数据层、逻辑层、表现层等。每一层负责不同的职能，保持了模块的独立性和高内聚性，降低了模块间的耦合度。数据层负责与数据源进行交互，确保数据的持久化和读取；逻辑层封装了业务逻辑，提供了清晰的接口和服务；表现层则专注于与用户的交互。分层设计不仅使得系统的维护和扩展更加方便，还提高了系统的可测试性。每个模块的功能相对独立，修改或扩展某一层的功能时，基本不影响其他层，减少了整体系统的复杂性。此外，分层架构还有助于优化性能，因为不同层次的优化和扩展可以独立进行，避免了全局修改带来的风险。

### 9.2 软件设计模式的应用

​	系统在设计过程中合理应用了多种软件设计模式，提升了系统的灵活性和可维护性。例如，工厂模式在对象的创建过程中解耦了客户端与具体类的关系，通过工厂方法提供对象的实例化，使得对象的创建和使用分离，提高了代码的灵活性；策略模式在实现多种算法或策略时，封装了不同策略的选择逻辑，可以在运行时根据需求切换策略，使得系统能够灵活应对不同的场景；设计模式的应用提高了代码的可复用性和可扩展性，避免了代码的冗余和重复，实现了模块之间的低耦合和高内聚。

### 9.3 人工智能算法设计与优化

​	系统在算法设计上遵循了性能和可扩展性并重的原则，采用了高效且灵活的算法来应对各种游戏逻辑。特别是在棋局评估和AI决策方面，系统主要通过蒙特卡罗树和博弈树结合的方式，使AI拥有较强的决策质量和智能化水平。在算法设计中，考虑到游戏对战的复杂性，系统通过提前对局面进行分析和预判，能够快速作出决策，并且在面对不确定局面时，能够通过模拟和随机性获得最优解。此外，在AI对战的过程中，系统通过启发式搜索和动态调整策略，重复利用计算机资源。对于复杂的计算，针对性优化了算法的时间复杂度。

### 9.4 算法的抽象与封装

​	系统中的核心算法经过了充分的抽象与封装，使得算法的实现对外部调用者透明。复杂的算法逻辑被封装成独立的模块，通过统一的接口与外部系统交互，这种封装不仅提升了代码的可读性和可维护性，也降低了系统的耦合度。例如，棋局评估和决策的算法被独立成模块，外部系统只需通过接口调用，无需关心具体实现细节。通过抽象和封装，算法可以被灵活替换或扩展，而不影响其他功能模块。此外，封装后的算法具有较强的复用性，可以在不同场景中被直接调用，避免了代码的重复实现。算法的抽象化处理，使得系统能够灵活应对未来可能的功能扩展和算法优化，保持系统的灵活性和可扩展性。



[AbstractFactory]: ./Design/img/AbstractFactory.png
[ACAutomationBuild]: ./Design/img/ACAutomationBuild.png
[AIproducts]: ./Design/img/AIproducts.png
[Anti-Go]: ./Design/img/Anti-Go.png
[GoBang]: ./Design/img/GoBang.png
[IterativeDeepening]: ./Design/img/IterativeDeepening.png
[KillBoardClassDiagram]: ./Design/img/KillBoardClassDiagram.png
[MCTSAlgorithmRunningFlow]: ./Design/img/MCTSAlgorithmRunningFlow.png
[MCTSBuild]: ./Design/img/MCTSBuild.png
[MCTSClassDiagram]: ./Design/img/MCTSClassDiagram.png
[MCTSFlow]: ./Design/img/MCTSFlow.png
[MCTSNodeClassDiagram]: ./Design/img/MCTSNodeClassDiagram.png
[MinMaxProductClassDiagram]: ./Design/img/MinMaxProductClassDiagram.png
[MinMaxSearch]: ./Design/img/MinMaxSearch.png
[Pruning]: ./Design/img/Pruning.png
[StrategyInterface]: ./Design/img/StrategyInterface.png
[SystemArchitectureDiagram]: ./Design/img/SystemArchitectureDiagram.png
[SystemModuleDiagram]: ./Design/img/SystemModuleDiagram.png
[UCBFunction]: ./Design/img/UCBFunction.png

[VCF1]: ./Design/img/VCF1.png
[VCF2]: ./Design/img/VCF2.png
[VCF3]: ./Design/img/VCF3.png
[VCF4]: ./Design/img/VCF4.png
[VCF5]: ./Design/img/VCF5.png
[VCT1]: ./Design/img/VCT1.png
[VCT2]: ./Design/img/VCT2.png
[VCT3]: ./Design/img/VCT3.png
[VCT4]: ./Design/img/VCT4.png
[VCT5]: ./Design/img/VCT5.png





