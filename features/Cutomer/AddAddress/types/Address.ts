export interface Address {
  id: number;
  title: string;
  governorate: string;
  city: string;
  district: string | null;
  street: string;
  buildingNumber: string | null;
  floorNumber: string | null;
  apartmentNumber: string | null;
  landmark: string | null;
  latitude: number | null;
  longitude: number | null;
  isDefault: boolean;
}
export interface CreateAddressDto {
  title: string;
  governorate: string;
  city: string;
  street: string;

  district?: string | null;
  buildingNumber?: string | null;
  floorNumber?: string | null;
  apartmentNumber?: string | null;
  landmark?: string | null;

  latitude?: number | null;
  longitude?: number | null;

  isDefault: boolean;
}

export interface UpdateAddressDto {
  title: string;
  governorate: string;
  city: string;
  street: string;

  district?: string | null;
  buildingNumber?: string | null;
  floorNumber?: string | null;
  apartmentNumber?: string | null;
  landmark?: string | null;

  latitude?: number | null;
  longitude?: number | null;
}