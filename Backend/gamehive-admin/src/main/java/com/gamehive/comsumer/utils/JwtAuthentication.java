package com.gamehive.comsumer.utils;

import javax.servlet.http.HttpServletRequest;
import org.springframework.web.context.request.RequestContextHolder;
import org.springframework.web.context.request.ServletRequestAttributes;
import com.gamehive.common.constant.Constants;
import com.gamehive.common.utils.StringUtils;
import com.gamehive.framework.web.service.TokenService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;

@Component
public class JwtAuthentication implements ApplicationContextAware {
    
    private static TokenService tokenService;
    
    @Override
    public void setApplicationContext(ApplicationContext applicationContext) {
        tokenService = applicationContext.getBean(TokenService.class);
    }

    /**
     * 获取当前登录用户ID
     * @return 用户ID，如果未登录返回-1
     */
    public static Long getUserId() {
        try {
            HttpServletRequest request = ((ServletRequestAttributes) RequestContextHolder.getRequestAttributes()).getRequest();
            String token = request.getHeader(Constants.TOKEN);
            if (StringUtils.isNotEmpty(token) && token.startsWith(Constants.TOKEN_PREFIX)) {
                token = token.replace(Constants.TOKEN_PREFIX, "");
            }
            if (StringUtils.isEmpty(token)) {
                return -1L;
            }
            return tokenService.getLoginUser(request).getUserId();
        } catch (Exception e) {
            return -1L;
        }
    }
}
