export interface PendingArtisan {
  userId: number;
  fullName: string;
  email: string | null;
  phoneNumber: string | null;
  profession: string;
  experienceYears: number;
  serviceArea: string;
  approvalStatus: string;
  accountStatus: string;
  createdAtUtc: string;
}
export interface AdminActionResponse {
  message: string;
  userId: number;
  approvalStatus: string;
  accountStatus: string;
  rejectionReason?: string | null;
}
