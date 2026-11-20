import {  type UseFormRegister } from "react-hook-form";

import type { ServiceRequestFormData } from "./ServiceRequestSchema";
import { ErrorMessage } from "./ErrorMessage";

interface Props {
  label: string;
  name: keyof ServiceRequestFormData;
  register: UseFormRegister<ServiceRequestFormData>;
  error?: string;
  placeholder?: string;
}

const FormInput = ({
  label,
  name,
  register,
  error,
  placeholder,
}: Props) => {
  return (
    <div>
      <label className="form-label">{label}</label>

      <input
        {...register(name)}
        className="form-input"
        placeholder={placeholder}
      />

      <ErrorMessage message={error} />
    </div>
  );
};

export default FormInput;