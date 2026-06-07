export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  role: string;
  fullName: string;
  userId: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface ServiceRoute {
  id: string;
  name: string;
  areaDescription: string;
  isActive: boolean;
  studentCount: number;
  createdAt: string;
}

export interface Student {
  id: string;
  fullName: string;
  schoolName: string;
  pickupAddress: string | null;
  inviteCode: string;
  inviteCodeUsed: boolean;
  isActive: boolean;
  routeId: string;
  createdAt: string;
}

export interface Trip {
  id: string;
  routeId: string;
  routeName: string;
  tripType: string;
  status: string;
  startedAt: string;
  endedAt: string | null;
  events: TripEvent[];
}

export interface TripEvent {
  studentId: string;
  studentName: string;
  eventType: string;
  latitude: number;
  longitude: number;
  timestamp: string;
}

export interface LiveLocation {
  tripId: string;
  latitude: number;
  longitude: number;
  timestamp: string;
  isSignalLost: boolean;
}

export interface User {
  userId: string;
  fullName: string;
  role: 'Driver' | 'Parent' | 'Admin';
  accessToken: string;
  refreshToken: string;
}