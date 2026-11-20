import axios from "axios";

const API_URL = "https://localhost:7136/api";

const getAuthConfig = () => {
  const token = localStorage.getItem("token")?.replace(/^"|"$/g, "");

  return {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  };
};

// ================= Wallet =================

// Get Wallet
export const getWallet = async () => {
  const response = await axios.get(
    `${API_URL}/Wallet/me`,
    getAuthConfig()
  );

  return response.data;
};

// ================= Top Ups =================

// Create Top Up
export const createTopUp = async (data: number) => {
  try{
    
      const response = await axios.post(
          `${API_URL}/WalletTopUps`,
          { amount: data },
          getAuthConfig()
        );
        
        return response.data;
    }catch(error){
        if (axios.isAxiosError(error)) {

            console.error("Error creating top-up:", error.response?.data || error.message);
        }
        throw error;
    }
};

// ================= Transactions =================

// Get Transactions
export const getTransactions = async () => {
  const response = await axios.get(
    `${API_URL}/Wallet/my-transactions`,
    getAuthConfig()
  );

  return response.data;
};

// ================= Payments =================

// Create Wallet Payment
export const createWalletPayment = async (
  requestId: number
) => {
  const response = await axios.post(
    `${API_URL}/WalletPayments/requests/${requestId}/pay`,
    {},
    getAuthConfig()
  );

  return response.data;
};