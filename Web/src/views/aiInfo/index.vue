<template>
  <div class="app-container">
    <!-- AI矩阵部分 -->
    <AIMatrix />
  </div>
</template>

<script>
import AIMatrix from "@/components/AIMatrix";

export default {
  name: "AIInfo",
  components: {
    AIMatrix
  }
};
</script>

<style scoped lang="scss">
.ranking-container {
  padding: 40px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.1);
  margin: 20px;
}

.ranking-title {
  text-align: center;
  margin-bottom: 30px;
  color: #303133;
  font-size: 24px;
  font-weight: bold;
}
</style>
