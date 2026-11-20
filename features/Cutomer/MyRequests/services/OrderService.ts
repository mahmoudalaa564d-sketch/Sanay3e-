import axios from "axios";

const API_URL = "https://localhost:7136/api/Customer/orders";

const getAuthConfig = () => {
  const storedToken = localStorage.getItem("token");

  const token = storedToken
    ? storedToken.replace(/^"|"$/g, "")
    : "";

  console.log("Token:", token);

  return {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  };
};

export const getOrders = async () => {
  try {
    const response = await axios.get(API_URL, getAuthConfig());

    console.log("Orders Response:", response);
    console.log("Orders Data:", response.data);

    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error("Status:", error.response?.status);
      console.error("Error Data:", error.response?.data);
      console.error("Error Message:", error.message);
    }

    throw error;
  }
};

export const getOrderById = async (id: number) => {
  const response = await axios.get(
    `${API_URL}/${id}`,
    getAuthConfig()
  );

  console.log("Order:", response.data);

  return response.data;
};

export const cancelOrder = async (id: number) => {
  const response = await axios.put(
    `${API_URL}/${id}/cancel`,
    {},
    getAuthConfig()
  );

  console.log("Cancelled:", response.data);

  return response.data;
};