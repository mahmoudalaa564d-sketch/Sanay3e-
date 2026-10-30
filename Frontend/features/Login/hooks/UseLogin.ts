import LoginService from "../api/LoginService";
import type { LoginFormData } from "../type/loginType";
import {useMutation}from"@tanstack/react-query";
export const UseLogin=()=>{
const login=new LoginService();
    return useMutation({
        mutationFn:(data:LoginFormData)=> login.login(data),
        onSuccess:data => data,
        onError:e=>e
    })
}