import React from 'react';
import { useTranslation } from 'react-i18next';
import type { SuuntoWorkout } from '../../types/suunto';

interface SuuntoWorkoutCardProps {
  workout: SuuntoWorkout;
}

export const SuuntoWorkoutCard: React.FC<SuuntoWorkoutCardProps> = ({ workout }) => {
  const { t } = useTranslation();
  
  const formatDuration = (seconds: number): string => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };
  
  const formatDate = (startTime: string): string => {
    const date = new Date(startTime);
    return date.toLocaleDateString(undefined, { 
      weekday: 'short', 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };
  
  const formatDistance = (meters: number): string => {
    return `${(meters / 1000).toFixed(1)} km`;
  };
  
  const getWorkoutIcon = (): string => {
    // Simple mapping - could be extended
    return '🏃';
  };
  
  const getZoneColor = (zone: number): string => {
    const colors = ['bg-gray-400', 'bg-blue-400', 'bg-green-400', 'bg-yellow-400', 'bg-red-400'];
    return colors[zone - 1] || colors[0];
  };
  
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 mb-4 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-3">
        <div className="flex items-center gap-2">
          <span className="text-2xl">{getWorkoutIcon()}</span>
          <div>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {formatDate(workout.startTime)}
            </p>
          </div>
        </div>
      </div>
      
      {/* Metrics */}
      <div className="grid grid-cols-3 gap-4 mb-4">
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide">
            {t('suunto.duration')}
          </p>
          <p className="text-lg font-semibold text-gray-900 dark:text-white">
            {formatDuration(workout.totalTimeSeconds)}
          </p>
        </div>
        
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide">
            {t('suunto.distance')}
          </p>
          <p className="text-lg font-semibold text-gray-900 dark:text-white">
            {formatDistance(workout.totalDistance)}
          </p>
        </div>
        
        <div className="text-center">
          <p className="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide">
            {t('suunto.calories')}
          </p>
          <p className="text-lg font-semibold text-orange-600 dark:text-orange-400">
            {workout.calories ? Math.round(workout.calories) : '--'} kcal
          </p>
        </div>
      </div>
      
      {/* Heart Rate */}
      {(workout.avgHeartRate || workout.maxHeartRate) && (
        <div className="flex justify-center gap-6 mb-4 text-sm">
          {workout.avgHeartRate && (
            <span className="text-gray-600 dark:text-gray-300">
              ❤️ {t('suunto.avgHr')}: <strong>{Math.round(workout.avgHeartRate)}</strong>
            </span>
          )}
          {workout.maxHeartRate && (
            <span className="text-gray-600 dark:text-gray-300">
              ❤️ {t('suunto.maxHr')}: <strong>{Math.round(workout.maxHeartRate)}</strong>
            </span>
          )}
        </div>
      )}
      
      {/* Heart Rate Zones */}
      {workout.heartRateZones.length > 0 && (
        <div className="mt-3">
          <p className="text-xs text-gray-500 dark:text-gray-400 mb-2 uppercase tracking-wide">
            {t('suunto.zones')}
          </p>
          <div className="flex h-4 rounded-full overflow-hidden">
            {workout.heartRateZones.map((zone) => (
              <div
                key={zone.zone}
                className={`${getZoneColor(zone.zone)} transition-all`}
                style={{ width: `${zone.percentage}%` }}
                title={`Zone ${zone.zone}: ${Math.round(zone.percentage)}%`}
              />
            ))}
          </div>
          <div className="flex justify-between mt-1 text-xs text-gray-400">
            <span>Z1</span>
            <span>Z2</span>
            <span>Z3</span>
            <span>Z4</span>
            <span>Z5</span>
          </div>
        </div>
      )}
      
      {/* Elevation */}
      {(workout.totalAscent > 0 || workout.totalDescent > 0) && (
        <div className="mt-3 pt-3 border-t border-gray-200 dark:border-gray-700 flex justify-center gap-6 text-sm text-gray-600 dark:text-gray-300">
          <span>↗️ {Math.round(workout.totalAscent)}m</span>
          <span>↘️ {Math.round(workout.totalDescent)}m</span>
        </div>
      )}
    </div>
  );
};
