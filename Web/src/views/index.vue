<template>
  <div class="app-container">
    <div class="welcome-container">
      <h1 class="welcome-title">欢迎使用GameHive系统</h1>
    </div>
  </div>
</template>

<script>
export default {
  name: "Index",
  data() {
    return {
      // 基础数据
    };
  },
  created() {
    // 初始化页面
  },
  methods: {
    // 基础方法
  }
};
</script>

<style scoped lang="scss">
.welcome-container {
  padding: 40px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.1);
  margin: 20px;
  text-align: center;
}

.welcome-title {
  color: #303133;
  font-size: 28px;
  font-weight: bold;
}
</style>


