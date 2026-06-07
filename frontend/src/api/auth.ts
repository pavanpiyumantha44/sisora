import apiClient from './client';
import type { ApiResponse, AuthResponse } from '../types';

export const registerDriver = (data: {
  fullName: string;
  phone: string;
  nic: string;
  password: string;
  vehicleModel: string;
  registrationNumber: string;
  vinNumber?: string;
  vehicleColor?: string;
  vehicleType: string;
}) => apiClient.post<ApiResponse<AuthResponse>>('/api/auth/driver/register', data);

export const registerParent = (data: {
  fullName: string;
  phone: string;
  password: string;
}) => apiClient.post<ApiResponse<AuthResponse>>('/api/auth/parent/register', data);

export const loginDriver = (data: {
  phone: string;
  password: string;
}) => apiClient.post<ApiResponse<AuthResponse>>('/api/auth/driver/login', data);

export const loginParent = (data: {
  phone: string;
  password: string;
}) => apiClient.post<ApiResponse<AuthResponse>>('/api/auth/parent/login', data);

export const loginAdmin = (data: {
  email: string;
  password: string;
}) => apiClient.post<ApiResponse<AuthResponse>>('/api/auth/admin/login', data);