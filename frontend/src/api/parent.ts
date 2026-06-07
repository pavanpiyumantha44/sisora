import apiClient from './client';
import type { ApiResponse, Student, Trip } from '../types';

export const redeemInviteCode = (inviteCode: string) =>
  apiClient.post<ApiResponse<Student>>('/api/parent/redeem-invite', { inviteCode });

export const getMyChildren = () =>
  apiClient.get<ApiResponse<Student[]>>('/api/parent/children');

export const getActiveTripForStudent = (studentId: string) =>
  apiClient.get<ApiResponse<Trip>>(`/api/parent/students/${studentId}/active-trip`);