export interface Specialty {
  id: number;
  name: string;
  nameEn: string;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface SpecialtyRequest {
  name: string;
  nameEn: string;
  description?: string | null;
  sortOrder: number;
}