import axios from "axios";
import type { CreateServiceRequestDto } from "../type/CreateServiceRequestDto";

class CustomerOrdersService {
  private endpoint = "https://localhost:7136/api/Customer/orders";
  private dashboardEndpoint =
    "https://localhost:7136/api/CustomerDashboard";

  private getHeaders() {
    const token = localStorage.getItem("token");

    return {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    };
  }

  // إنشاء طلب خدمة
  async createOrder(data: CreateServiceRequestDto) {
    const response = await axios.post(this.endpoint, data, {
      headers: this.getHeaders(),
    });

    return response.data;
  }

  // جلب التخصصات
  async getSpecialties() {
    const response = await axios.get(
      `${this.dashboardEndpoint}/specialties`,
      {
        headers: this.getHeaders(),
      }
    );

    return response.data;
  }
}

export default new CustomerOrdersService();