package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.TestPropertySource;

import static org.junit.jupiter.api.Assertions.*;

/**
 * DeepSeek AI服务测试类
 * 测试大模型在各种游戏场景下的决策效果
 * 
 * @author Cassifa
 * @since 1.0.0
 */
@SpringBootTest
@TestPropertySource(properties = {
    "lmm.max-retry-count=3",
    "spring.ai.dashscope.api-key=${DASHSCOPE_API_KEY:sk-4e208c79817945e9b2f62217a82a17ff}"
})
public class DeepSeekAIServiceTest {

    @Autowired
    private DeepSeekAIServiceImpl deepSeekAIService;

    /**
     * 测试井字棋开局场景
     * 空棋盘，AI应该选择中心位置或角落位置
     */
    @Test
    void testTicTacToe_Opening() {
        LMMRequestDTO request = new LMMRequestDTO(
                1001, // gameId
                1L,
                "000\n000\n000",
                "1",
                "井字棋",
                "标准井字棋规则，三子连线获胜",
                "",
                "3"
        );
        
        System.out.println("=== 测试井字棋开局场景 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1001);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertNotNull(result.getX(), "X坐标不能为空");
        assertNotNull(result.getY(), "Y坐标不能为空");
        assertTrue(result.isValid(request), "决策结果应该有效");
        
        // 开局通常选择中心(1,1)或角落位置
        boolean isGoodOpening = (result.getX() == 1 && result.getY() == 1) || // 中心
                                (result.getX() == 0 && result.getY() == 0) || // 左上角
                                (result.getX() == 0 && result.getY() == 2) || // 右上角
                                (result.getX() == 2 && result.getY() == 0) || // 左下角
                                (result.getX() == 2 && result.getY() == 2);   // 右下角
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
        System.out.println("策略分析: " + analyzeMove(result, request));
    }

    /**
     * 测试井字棋防守场景
     * 对手即将获胜，AI应该阻止
     */
    @Test
    void testTicTacToe_DefensiveMove() {
        LMMRequestDTO request = new LMMRequestDTO(
                1002, // gameId
                2L,
                "110\n000\n000",
                "2",
                "井字棋",
                "标准井字棋规则，三子连线获胜。你是玩家2，需要阻止玩家1获胜",
                "玩家1已经在第一行放了两个棋子",
                "3"
        );
        
        System.out.println("=== 测试井字棋防守场景 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1002);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertTrue(result.isValid(request), "决策结果应该有效");
        
        // AI应该选择(0,2)来阻止对手获胜
        boolean isDefensiveMove = (result.getX() == 0 && result.getY() == 2);
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
        
        if (isDefensiveMove) {
            System.out.println("✅ AI正确识别并阻止了对手的获胜机会");
        } else {
            System.out.println("⚠️ AI可能错过了防守机会");
        }
    }

    /**
     * 测试井字棋进攻场景
     * AI有机会获胜，应该选择获胜位置
     */
    @Test
    void testTicTacToe_WinningMove() {
        LMMRequestDTO request = new LMMRequestDTO(
                1003, // gameId
                3L,
                "220\n100\n000",
                "2",
                "井字棋",
                "标准井字棋规则，三子连线获胜。你是玩家2",
                "你已经在第一行放了两个棋子，可以获胜",
                "3"
        );
        
        System.out.println("=== 测试井字棋进攻场景 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1003);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertTrue(result.isValid(request), "决策结果应该有效");
        
        // AI应该选择(0,2)来获胜
        boolean isWinningMove = (result.getX() == 0 && result.getY() == 2);
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
        
        if (isWinningMove) {
            System.out.println("✅ AI正确识别并选择了获胜位置");
        } else {
            System.out.println("⚠️ AI错过了获胜机会");
        }
    }

    /**
     * 测试复杂的井字棋中局场景
     * 多种选择，测试AI的策略思考
     */
    @Test
    void testTicTacToe_ComplexMidGame() {
        LMMRequestDTO request = new LMMRequestDTO(
                1004, // gameId
                4L,
                "120\n010\n000",
                "2",
                "井字棋",
                "标准井字棋规则，三子连线获胜。你是玩家2",
                "中局阶段，需要考虑多种可能性",
                "3"
        );
        
        System.out.println("=== 测试井字棋复杂中局 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1004);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertTrue(result.isValid(request), "决策结果应该有效");
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
        System.out.println("策略分析: " + analyzeMove(result, request));
    }

    /**
     * 测试4x4棋盘的五子棋场景
     * 测试AI在更大棋盘上的表现
     */
    @Test
    void testFiveInRow_4x4Board() {
        LMMRequestDTO request = new LMMRequestDTO(
                1005, // gameId
                5L,
                "1200\n0210\n0000\n0000",
                "1",
                "四子棋",
                "四子连线规则，四个棋子连成一线获胜",
                "4x4棋盘，需要四子连线",
                "4"
        );
        
        System.out.println("=== 测试4x4四子棋场景 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1005);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertTrue(result.isValid(request), "决策结果应该有效");
        assertTrue(result.getX() >= 0 && result.getX() < 4, "X坐标应该在0-3范围内");
        assertTrue(result.getY() >= 0 && result.getY() < 4, "Y坐标应该在0-3范围内");
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
    }

    /**
     * 测试棋盘接近满盘的情况
     * 验证AI能否在有限选择中做出合理决策
     */
    @Test
    void testNearFullBoard() {
        LMMRequestDTO request = new LMMRequestDTO(
                1006, // gameId
                6L,
                "121\n212\n120",
                "1",
                "井字棋",
                "标准井字棋规则，三子连线获胜",
                "棋盘快满了，只剩少数位置",
                "3"
        );
        
        System.out.println("=== 测试接近满盘场景 ===");
        LMMDecisionResult result = deepSeekAIService.getDecision(request, 1006);
        
        assertNotNull(result, "大模型应该返回决策结果");
        assertTrue(result.isValid(request), "决策结果应该有效");
        
        // 只有(2,2)位置可选
        boolean isCorrectMove = (result.getX() == 2 && result.getY() == 2);
        
        System.out.println("AI选择位置: (" + result.getX() + "," + result.getY() + ")");
        System.out.println("决策理由: " + result.getReason());
        
        if (isCorrectMove) {
            System.out.println("✅ AI正确识别了唯一可用位置");
        } else {
            System.out.println("⚠️ AI选择了错误的位置");
        }
    }

    /**
     * 分析AI决策的合理性
     */
    private String analyzeMove(LMMDecisionResult result, LMMRequestDTO request) {
        int x = result.getX();
        int y = result.getY();
        
        // 对于3x3井字棋的分析
        if ("3".equals(request.getGridSize())) {
            if (x == 1 && y == 1) {
                return "选择中心位置，控制全局";
            } else if ((x == 0 || x == 2) && (y == 0 || y == 2)) {
                return "选择角落位置，建立优势";
            } else {
                return "选择边缘位置，可能是战术性选择";
            }
        }
        
        return "决策位置: (" + x + "," + y + ")";
    }
} 
