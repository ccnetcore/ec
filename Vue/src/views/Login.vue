
<template>
  <div class="login-box">
    <!--head-->
    <div class="py-container logoArea">
      <a href="" class="logo"></a>
    </div>
    <!--loginArea-->
    <div class="loginArea" id="loginApp">
      <div class="py-container login">
        <div class="loginform">
          <ul class="sui-nav nav-tabs tab-wraped">
            <li>
              <a href="#index" data-toggle="tab">
                <h3>扫描登录</h3>
              </a>
            </li>
            <li class="active">
              <a href="#profile" data-toggle="tab">
                <h3>账户登录</h3>
              </a>
            </li>
          </ul>
          <div class="tab-content tab-wraped">
            <div id="index" class="tab-pane">
              <p>二维码登录，暂为官网二维码</p>
              <img src="img/wx_cz.jpg" />
            </div>
            <div id="profile" class="tab-pane active">
              <span v-text="msg"></span>
              <form class="sui-form">
                <div class="input-prepend">
                  <span class="add-on loginname"></span>
                  <input
                    id="username"
                    type="text"
                    value="clay"
                    placeholder="邮箱/用户名/手机号"
                    class="span2 input-xfat"
                    v-model="form.username"
                  />
                </div>
                <div class="input-prepend">
                  <span class="add-on loginpwd"></span>
                  <input
                    id="password"
                    type="password"
                    value="123456"
                    placeholder="请输入密码"
                    class="span2 input-xfat"
                    v-model="form.password"
                  />
                </div>
                <div class="setting">
                  <label class="checkbox inline">
                    <input name="m1" type="checkbox" value="2" checked="" />
                    自动登录
                  </label>
                  <span class="forget">忘记密码？</span>
                </div>
                <div class="logined">
                  <a
                    class="sui-btn btn-block btn-xlarge btn-danger"
                    @click="login"
                    href="javascript:void(0)"
                    >登&nbsp;&nbsp;录</a
                  >
                </div>
              </form>
              <div class="otherlogin">
                <div class="types">
                  <ul>
                    <li><img src="img/qq.png" width="35px" height="35px" /></li>
                    <li><img src="img/sina.png" /></li>
                    <li><img src="img/ali.png" /></li>
                    <li><img src="img/weixin.png" /></li>
                  </ul>
                </div>
                <span class="register"
                  >
                  <router-link :to="{path:'/register'}">立即注册</router-link>
            </span
                >
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!--foot-->

    <!-- <script
      type="text/javascript"
      src="js/plugins/jquery/jquery.min.js"
    ></script>
    底部栏位
    页面底部版权信息，由js动态加载
    <div class="Mod-copyright"></div>
    <script type="text/javascript">
      $(".Mod-copyright").load("copyright.html");
    </script>
    页面底部END -->
  </div>

  <!-- <script type="text/javascript" src="js/plugins/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="js/plugins/jquery.easing/jquery.easing.min.js"></script>
    <script type="text/javascript" src="js/plugins/sui/sui.min.js"></script>
    <script type="text/javascript" src="js/plugins/jquery-placeholder/jquery.placeholder.min.js"></script>
    <script type="text/javascript" src="js/pages/login.js"></script> -->
</template>
<script>
import accountApi from "@/api/accountApi";
export default {
  data(){
      return{
    form: {
      username: "",
      password: "",
    },
    msg: "",
      }

  },
  methods: {
    login() {
      accountApi.login(this.form)
        .then((resp) => {
          if (resp.status) {
            let token = resp.data; // 获取token
            alert("登录成功"+token);
            localStorage.setItem("token", token); // token存入本地缓存
            this.$router.push('/')
            // const url = yitao.getUrlParam("returnUrl");
            // window.location = url || "http://" + window.location.host;
          } else {
            this.msg = "用户名或密码错误";
          }
        })
    },
  }
};
</script>

