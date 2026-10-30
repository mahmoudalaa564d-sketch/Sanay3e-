import { useMutation } from "@tanstack/react-query"
import { RegisterService } from "../api/RegisterService"
import { RegisterPayload } from "../type/RegisterPayload";

export const UseRegister=()=>{
const register=new RegisterService();
  return useMutation({
    mutationFn:(data:RegisterPayload)=>register.Register(data),
    onSuccess:data=>data,
    onError:e=>e
  })
}