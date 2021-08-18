import myaxios from '@/utils/myaxios'
export default {
    login(form) {
        return myaxios({
            url: '/auth/accredit',
            method: 'post',
            data: form
        })
    },
    sendCode(phone) {
        return myaxios({
            url: `/user/send?phone=${phone}`,
            method: 'post',
        })
    },
    register(user, code) {
        return myaxios({
            url: `/user/register?code=${code}`,
            method: 'post',
            data: user
        })
    },
    verify() {
        return myaxios({
            url: `/user/verify`,
            method: 'get',
        })
    }
}