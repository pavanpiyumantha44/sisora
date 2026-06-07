import apiClient from './client';
import type { ApiResponse, ServiceRoute, Student } from '../types';

export const getMyRoutes = () =>
  apiClient.get<ApiResponse<ServiceRoute[]>>('/api/driver/routes');

export const createRoute = (data: { name: string; areaDescription: string }) =>
  apiClient.post<ApiResponse<ServiceRoute>>('/api/driver/routes', data);

export const updateRoute = (routeId: string, data: { name: string; areaDescription: string }) =>
  apiClient.put<ApiResponse<ServiceRoute>>(`/api/driver/routes/${routeId}`, data);

export const deleteRoute = (routeId: string) =>
  apiClient.delete<ApiResponse<boolean>>(`/api/driver/routes/${routeId}`);

export const getStudents = (routeId: string) =>
  apiClient.get<ApiResponse<Student[]>>(`/api/driver/routes/${routeId}/students`);

export const addStudent = (routeId: string, data: {
  fullName: string;
  schoolName: string;
  pickupAddress?: string;
}) => apiClient.post<ApiResponse<Student>>(`/api/driver/routes/${routeId}/students`, data);

export const removeStudent = (routeId: string, studentId: string) =>
  apiClient.delete<ApiResponse<boolean>>(`/api/driver/routes/${routeId}/students/${studentId}`);

export const regenerateInviteCode = (routeId: string, studentId: string) =>
  apiClient.post(`/api/driver/routes/${routeId}/students/${studentId}/regenerate-code`);