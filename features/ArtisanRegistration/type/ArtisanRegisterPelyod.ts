export type ArtisanNearbyArea = {
  governorate: string;
  center: string;
  village: string;
};

export type ArtisanRegisterPayload = {
  fullName: string;
  email?: string;
  phoneNumber?: string;
  password: string;

  profession: string;
  experienceYears: number;
  bio: string;

  governorate: string;
  center: string;
  village: string;

  nationalIdFrontImage: File;
  nationalIdBackImage: File;
  profileImage: File;

  specialties: string;
  serviceTypes: string;

  termsAccepted: boolean;
};