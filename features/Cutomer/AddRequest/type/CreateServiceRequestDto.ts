export interface CreateServiceRequestDto {
  specialtyId: number;
  serviceTypeName: string;
  problemDescription: string;
  additionalNotes?: string | null;
  isAsSoonAsPossible: boolean;
  scheduledAtUtc?: string | null;

  addressTitle: string;
  governorate: string;
  city: string;
  district?: string | null;
  street: string;
  buildingNumber?: string | null;
  floorNumber?: string | null;
  apartmentNumber?: string | null;
  landmark?: string | null;

  latitude?: number | null;
  longitude?: number | null;
}
