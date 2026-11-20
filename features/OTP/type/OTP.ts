export interface RequestOtpRequest {
  userId: number;
  channel: string;
}

export interface CheckOtpRequest {
  userId: number;
  channel: string;
  code: string;
}
