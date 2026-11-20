
import axios from 'axios';
import type { LoginFormData } from '../type/loginType';

const API_URL = 'https://localhost:7136/api/Auth/login';

export default class LoginService {
  async login(credentials: LoginFormData) {
    const input = credentials.identifier.trim();
    const isPhone = /^\+?\d+$/.test(input);

    const payload = {
      ...(isPhone
        ? { phoneNumber: input }
        : { email: input }),
      password: credentials.password,
    };

    const response = await axios.post(API_URL, payload);

    return response.data;
  }
}
