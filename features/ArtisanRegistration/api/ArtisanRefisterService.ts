import axios from "axios";
import type { ArtisanRegisterPayload } from "../type/ArtisanRegisterPelyod";


export class ArtisanRegisterService {
  async RegisterArtisan(data: ArtisanRegisterPayload) {
    const formData = new FormData();

    formData.append("FullName", data.fullName);
if (data.email) {
  formData.append("Email", data.email);
}

if (data.phoneNumber) {
  formData.append("PhoneNumber", data.phoneNumber);
}
    formData.append("Password", data.password);

    formData.append("Profession", data.profession);
    formData.append("ExperienceYears", String(data.experienceYears));
    formData.append("Bio", data.bio);

    formData.append("Governorate", data.governorate);
    formData.append("Center", data.center);
    formData.append("Village", data.village);

    formData.append("NationalIdFrontImage", data.nationalIdFrontImage);
    formData.append("NationalIdBackImage", data.nationalIdBackImage);
    formData.append("ProfileImage", data.profileImage);

    formData.append("Specialties", data.specialties);
    formData.append("ServiceTypes", data.serviceTypes);

    formData.append("TermsAccepted", String(data.termsAccepted));

    const response = await axios.post(
      "https://localhost:7136/api/Auth/register-artisan",
      formData,
    );

    return response.data;
  }
}