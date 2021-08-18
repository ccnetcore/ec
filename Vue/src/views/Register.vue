<template>
  <div class="register py-container">
    <!--head-->
    <div class="logoArea">
      <a href="" class="logo"></a>
    </div>
    <!--register-->
    <div class="registerArea" id="registerApp">
      <h3>
        注册新用户<span class="go"
          >我有账号，去 <router-link :to="{path:'/login'}">登录</router-link></span
        >
      </h3>
      <div class="info" style="width: 650px">
        <form class="sui-form form-horizontal">
          <div class="control-group">
            <label class="control-label">用户名：</label>
            <div class="controls">
              <input
                type="text"
                placeholder="请输入你的用户名"
                class="input-xfat input-xlarge"
                v-model.lazy="user.username"
                name="username"

              
              />
            </div>
            <!-- <span style="color: red">{{ errors.first("username") }}</span> -->
          </div>
          <div class="control-group">
            <label class="control-label">登录密码：</label>
            <div class="controls">
              <input
                type="password"
                placeholder="设置登录密码"
                class="input-xfat input-xlarge"
                v-model="user.password"
                name="password"

              />
            </div>
            <!-- <span style="color: red">{{ errors.first("password") }}</span> -->
          </div>
          <div class="control-group">
            <label class="control-label">确认密码：</label>
            <div class="controls">
              <input
                type="password"
                placeholder="再次确认密码"
                class="input-xfat input-xlarge"
                v-model="user.confirmPassword"
                name="confirmPass"

              />
            </div>
            <!-- <span style="color: red">{{ errors.first("confirmPass") }}</span> -->
          </div>

          <div class="control-group">
            <label class="control-label">手机号：</label>
            <div class="controls">
              <input
                type="text"
                placeholder="请输入你的手机号"
                class="input-xfat input-xlarge"
                v-model="user.phone"
                name="phone"

              />
            </div>
            <!-- <span style="color: red">{{ errors.first("phone") }}</span> -->
          </div>
          <div class="control-group">
            <label class="control-label">短信验证码：</label>
            <div class="controls">
              <input
                type="text"
                placeholder="短信验证码"
                class="input-xfat input-xlarge"
                style="width: 120px"
                v-model="code"
      
             
              />
              <span class="code-span" @click="createVerifyCode">
                获取短信验证码
              </span>
            </div>
            <!-- <span style="color: red">{{ errors.first("code") }}</span> -->
          </div>

          <div class="control-group">
            <label class="control-label"
              >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</label
            >
            <div class="controls">
              <input name="m1" type="checkbox" value="2" checked="" /><span
                >同意协议并注册《易淘用户协议》</span
              >
            </div>
          </div>
          <div class="control-group">
            <label class="control-label"></label>
            <div class="controls btn-reg">
              <a
                class="sui-btn btn-block btn-xlarge btn-danger"
                href="javascript:void(0)"
                @click.stop="submit"
                >完成注册</a
              >
            </div>
          </div>
        </form>
        <div class="clearfix"></div>
      </div>
    </div>
  </div>
</template>
<script>
import accountApi from "@/api/accountApi";
export default {
  data() {
    return {
      code: "",
      user: {
        username: "",
        password: "",
        confirmPassword: "",
        phone: "",
      },
    };
  },
//   created() {
//     this.$validator.extend("useful", {
//       getMessage(field, args, data) {
//         return args[0] === "1" ? "用户名" + data : "手机" + data;
//       },
//       validate(value, args) {
//         return new Promise((resolve) => {
//           yitao.http
//             .get("/user/check/" + value + "/" + args[0])
//             .then((resp) => {
//               resolve({
//                 valid: resp.data.result,
//                 data: "已存在!",
//               });
//             });
//         });
//       },
//     });
//     this.$validator.extend("confirm", {
//       getMessage() {
//         return "两次密码不一致";
//       },
//       validate(val, args) {
//         return val === args[0];
//       },
//     });
//   },
  methods: {
    createVerifyCode() {
      accountApi.sendCode(this.user.phone).then((resp) => {
        alert(resp.msg);
      });
      //   // 生成短信验证码
      //   this.$validator.validate("phone").then((r) => {
      //     if (r) {
      //       yitao.http
      //         .post("/user/send", "phone=" + this.user.phone)
      //         .then((resp) => {
      //           if (resp.data.result == true) {
      //             alert(resp.data.message);
      //           } else {
      //             alert(resp.data.message);
      //           }
      //         })
      //         .catch(() => alert("注册失败！"));
      //     }
      //   });
    },
    submit() {
      accountApi.register(this.user, this.code).then((resp) => {
        if (resp.status) {
          alert("注册成功！");
        } else {
          alert(resp.msg);
        }
      });

      //   this.$validator.validateAll().then((d) => {
      //     if (d) {
      //       // 校验通过，提交表单
      //       yitao.http
      //         .post("/user/register", yitao.stringify(this.user))
      //         .then((resp) => {
      //           if (resp.data.result == true) {
      //             alert("注册成功,即将跳转到登录页！"); // 注册成功
      //             setTimeout(() => (window.location = "/login.html"), 2000);
      //           } else {
      //             alert(resp.data.message);
      //           }
      //         })
      //         .catch(() => alert("注册失败！"));
      //     }
      //   });
    },
  },
};
</script>
<style scoped>
.code-span {
  display: inline-block;
  border: 1px solid #999;
  text-align: center;
  line-height: 35px;
  width: 118px;
  height: 35px;
  float: right;
  margin-left: 10px;
  cursor: pointer;
}

.code-span:hover {
  background-color: #c0ccda;
}
</style>