import apiClient from './client';
import type { ApiResponse, Trip, TripEvent, LiveLocation } from '../types';

export const startTrip = (routeId: string, tripType: string) =>
  apiClient.post<ApiResponse<Trip>>(`/api/trip/routes/${routeId}/start`, { tripType });

export const endTrip = (tripId: string) =>
  apiClient.post<ApiResponse<Trip>>(`/api/trip/${tripId}/end`);

export const updateLocation = (tripId: string, latitude: number, longitude: number) =>
  apiClient.post<ApiResponse<boolean>>(`/api/trip/${tripId}/location`, { latitude, longitude });

export const recordTripEvent = (tripId: string, data: {
  studentId: string;
  latitude: number;
  longitude: number;
}) => apiClient.post<ApiResponse<TripEvent>>(`/api/trip/${tripId}/events`, data);

export const getLiveLocation = (tripId: string) =>
  apiClient.get<ApiResponse<LiveLocation>>(`/api/trip/${tripId}/location`);

export const getActiveTrip = (routeId: string) =>
  apiClient.get<ApiResponse<Trip>>(`/api/trip/routes/${routeId}/active`);

export const getTripHistory = (routeId: string) =>
  apiClient.get<ApiResponse<Trip[]>>(`/api/trip/routes/${routeId}/history`);