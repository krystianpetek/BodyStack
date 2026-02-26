export interface SuuntoWorkout {
  activityId: number;
  startTime: string;
  totalTimeSeconds: number;
  totalDistance: number;
  totalAscent: number;
  totalDescent: number;
  calories?: number;
  avgHeartRate?: number;
  maxHeartRate?: number;
  workoutType?: string;
  stepCount?: number;
  heartRateZones: HeartRateZone[];
  extensions?: WorkoutExtensions;
}

export interface HeartRateZone {
  zone: number;
  lowerLimit: number;
  totalTimeSeconds: number;
  percentage: number;
}

export interface WorkoutExtensions {
  maxHeartRate?: number;
  vo2Max?: number;
  peakEpoc?: number;
  recoveryTime?: number;
  minTemperature?: number;
  avgTemperature?: number;
  maxTemperature?: number;
  feeling?: string;
}

export interface SuuntoWorkoutsResponse {
  workouts: SuuntoWorkout[];
  totalCount: number;
  totalCalories: number;
}

export interface SuuntoDailyEnergySummary {
  date: string;
  bmrCalories: number;
  activityCalories: number;
  workoutCalories: number;
  totalCalories: number;
}

export interface UserProfile {
  weightKg: number;
  heightCm: number;
  age: number;
  gender: 'male' | 'female';
}

export interface SuuntoUserProfile {
  weightKg: number;
  heightCm: number;
  age: number;
  gender: string;
  name?: string;
  email?: string;
}
