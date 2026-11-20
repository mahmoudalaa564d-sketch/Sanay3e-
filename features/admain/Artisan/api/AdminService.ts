import axios from "axios";
import type { AdminActionResponse, PendingArtisan } from "../type/pendingArtisan";

const API_URL = "https://localhost:7136/api";



class AdminService {
  private getAuthConfig() {
    const token = localStorage.getItem("token");

    if (!token) {
      throw new Error("Admin token not found");
    }

    return {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    };
  }

  // Get Pending Artisans
  async getPendingArtisans(): Promise<PendingArtisan[]> {
    const response = await axios.get<PendingArtisan[]>(
      `${API_URL}/Admin/pending-artisans`,
      this.getAuthConfig()
    );

    return response.data;
  }

  // Approve Artisan
  async approveArtisan(userId: number): Promise<AdminActionResponse> {
    const response = await axios.put<AdminActionResponse>(
      `${API_URL}/Admin/artisans/${userId}/approve`,
      {},
      this.getAuthConfig()
    );

    return response.data;
  }

  // Reject Artisan
  async rejectArtisan(
    userId: number,
    reason: string
  ): Promise<AdminActionResponse> {
    const response = await axios.put<AdminActionResponse>(
      `${API_URL}/Admin/artisans/${userId}/reject`,
      { reason },
      this.getAuthConfig()
    );

    return response.data;
  }
}

export default new AdminService(); 