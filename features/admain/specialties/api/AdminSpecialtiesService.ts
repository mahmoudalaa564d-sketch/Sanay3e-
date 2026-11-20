import axios from "axios";

import type { SpecialtyRequest } from "../type/specialty";

const API_URL = "https://localhost:7136/api/Admin/specialties";

class SpecialtyService {
private getHeaders() {
  const token = localStorage.getItem("token");

  return {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };
}

  // Get all specialties
  async getSpecialties() {
    try {
      const response = await axios.get(API_URL, {
        headers: this.getHeaders(),
      });

      return response.data;
    } catch (error: unknown) {
      console.error("Get Specialties Error:", error);
      throw error;
    }
  }

  // Create specialty
  async createSpecialty(data: SpecialtyRequest) {
    try {
      const response = await axios.post(API_URL, data, {
        headers: this.getHeaders(),
      });

      return response.data;
    } catch (error: unknown) {
      console.error("Create Specialty Error:", error);
      throw error;
    }
  }

  // Update specialty
  async updateSpecialty(id: number, data: SpecialtyRequest) {
    try {
      const response = await axios.put(`${API_URL}/${id}`, data, {
        headers: this.getHeaders(),
      });

      return response.data;
    } catch (error: unknown) {
      console.error("Update Specialty Error:", error);
      throw error;
    }
  }

  // Delete specialty
  async deleteSpecialty(id: number) {
    try {
      const response = await axios.delete(`${API_URL}/${id}`, {
        headers: this.getHeaders(),
      });

      return response.data;
    } catch (error: unknown) {
      console.error("Delete Specialty Error:", error);
      throw error;
    }
  }

  // Change specialty status
  async changeStatus(id: number, data: { isActive: boolean }) {
    try {
      const response = await axios.patch(
        `${API_URL}/${id}/status`,
        data,
        {
          headers: this.getHeaders(),
        }
      );

      return response.data;
    } catch (error: unknown) {
      console.error("Change Specialty Status Error:", error);
      throw error;
    }
  }
}

export default new SpecialtyService();
