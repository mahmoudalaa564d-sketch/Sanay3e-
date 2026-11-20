import axios from "axios";
import type {
  Address,
  CreateAddressDto,
  UpdateAddressDto,
} from "../types/Address";

const API_URL = "https://localhost:7136/api/Customer/addresses";

const getAuthConfig = () => {
  const token = localStorage.getItem("token");

  return {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  };
};

class AddressService {
  async getAll() {
    const response = await axios.get(
      API_URL,
      getAuthConfig()
    );

    console.log("Addresses API Response:", response.data);

    return response.data;
  }

async create(address: CreateAddressDto): Promise<Address> {
  try {
    console.log("Sending Address:", address);

    const response = await axios.post<Address>(
      `${API_URL}`,
      address,
      getAuthConfig()
    );

    return response.data;

  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.log("Status:", error.response?.status);
      console.log("Backend Error:", error.response?.data);
    }

    throw error;
  }
}

  async update(
    id: number,
    address: UpdateAddressDto
  ): Promise<Address> {
    const response = await axios.put<Address>(
      `${API_URL}/${id}`,
      address,
      getAuthConfig()
    );

    return response.data;
  }

  async delete(id: number) {
    const response = await axios.delete(
      `${API_URL}/${id}`,
      getAuthConfig()
    );

    return response.data;
  }

  async setDefault(id: number) {
    const response = await axios.put(
      `${API_URL}/${id}/default`,
      {},
      getAuthConfig()
    );

    return response.data;
  }
}

export default new AddressService();