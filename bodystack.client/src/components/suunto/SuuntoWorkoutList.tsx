import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { SuuntoWorkoutCard } from './SuuntoWorkoutCard';
import { getSuuntoWorkouts } from '../../api/suuntoApi';
import { useIntegrationsAuth } from '../../hooks/useIntegrationsAuth';
import type { SuuntoWorkout } from '../../types/suunto';

export const SuuntoWorkoutList: React.FC = () => {
  const { t } = useTranslation();
  const { getSuuntoKey, suunto } = useIntegrationsAuth();
  const [sttAuthorization, setSttAuthorization] = useState<string | null>(getSuuntoKey());

  const [workouts, setWorkouts] = useState<SuuntoWorkout[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCalories, setTotalCalories] = useState(0);

  useEffect(() => {
    setSttAuthorization(getSuuntoKey());
  }, [suunto, getSuuntoKey]);

  useEffect(() => {
    if (!sttAuthorization) return;

    loadWorkouts();
  }, [sttAuthorization]);

  const loadWorkouts = async () => {
    if (!sttAuthorization) return;
    
    setIsLoading(true);
    setError(null);
    
    try {
      // Load last 30 days by default
      const to = new Date().toISOString().split('T')[0];
      const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
      
      const response = await getSuuntoWorkouts({
        sttAuthorization,
        from,
        to,
        ttlMinutes: 15
      });
      
      setWorkouts(response.workouts);
      setTotalCalories(response.totalCalories);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load workouts');
    } finally {
      setIsLoading(false);
    }
  };
  
  if (!sttAuthorization) {
    return (
      <div className="text-center py-8 text-gray-500 dark:text-gray-400">
        {t('suunto.pleaseLogin')}
      </div>
    );
  }
  
  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-red-500 dark:text-red-400 mb-4">{error}</p>
        <button
          onClick={loadWorkouts}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
        >
          {t('common.retry')}
        </button>
      </div>
    );
  }
  
  if (workouts.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500 dark:text-gray-400">
        {t('suunto.noWorkouts')}
      </div>
    );
  }
  
  return (
    <div className="w-full max-w-2xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
          {t('suunto.workouts')}
        </h2>
        <div className="text-right">
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {workouts.length} {t('suunto.workoutsCount')}
          </p>
          <p className="text-sm font-medium text-orange-600 dark:text-orange-400">
            {Math.round(totalCalories)} kcal
          </p>
        </div>
      </div>
      
      <div className="space-y-4">
        {workouts.map((workout) => (
          <SuuntoWorkoutCard key={workout.activityId} workout={workout} />
        ))}
      </div>
    </div>
  );
};
