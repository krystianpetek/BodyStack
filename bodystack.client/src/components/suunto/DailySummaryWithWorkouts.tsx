import React, { useEffect, useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { getSuuntoWorkouts, getSuuntoUserProfile } from '../../api/suuntoApi';
import { useIntegrationsAuth } from '../../hooks/useIntegrationsAuth';
import type { SuuntoWorkout, SuuntoUserProfile } from '../../types/suunto';

interface DaySummary {
  date: string;
  activityCalories: number;
  bmrCalories: number;
  workoutCalories: number;
  totalCalories: number;
  workouts: SuuntoWorkout[];
}

interface DailySummaryWithWorkoutsProps {
  activityDays: Array<{
    date: string;
    energyConsumption: number;
  }>;
}

export const DailySummaryWithWorkouts: React.FC<DailySummaryWithWorkoutsProps> = ({ activityDays }) => {
  const { t } = useTranslation();
  const { getSuuntoKey } = useIntegrationsAuth();
  const sttAuthorization = getSuuntoKey();

  const [workouts, setWorkouts] = useState<SuuntoWorkout[]>([]);
  const [userProfile, setUserProfile] = useState<SuuntoUserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!sttAuthorization) return;

    loadData();
  }, [sttAuthorization]);

  const loadData = async () => {
    if (!sttAuthorization) return;

    setIsLoading(true);
    setError(null);

    try {
      // Pobierz profil użytkownika
      const profile = await getSuuntoUserProfile({ sttAuthorization });
      setUserProfile(profile);

      // Pobierz workouty z ostatnich 30 dni
      const to = new Date().toISOString().split('T')[0];
      const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

      const workoutsResponse = await getSuuntoWorkouts({
        sttAuthorization,
        from,
        to,
        ttlMinutes: 15
      });

      setWorkouts(workoutsResponse.workouts);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
    } finally {
      setIsLoading(false);
    }
  };

  // Grupuj workouty po dacie
  const workoutsByDate = useMemo(() => {
    const grouped = new Map<string, SuuntoWorkout[]>();

    workouts.forEach(workout => {
      const date = new Date(workout.startTime).toISOString().split('T')[0];
      if (!grouped.has(date)) {
        grouped.set(date, []);
      }
      grouped.get(date)!.push(workout);
    });

    return grouped;
  }, [workouts]);

  // Oblicz BMR (zakładamy stałą wartość na razie - można przeliczyć z profilu)
  const bmrPerDay = useMemo(() => {
    if (!userProfile) return 1641; // Default BMR

    // Mifflin-St Jeor Equation
    // BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) + 5 (male) / -161 (female)
    const { weightKg, heightCm, age, gender } = userProfile;
    let bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age);
    bmr = gender.toLowerCase() === 'female' ? bmr - 161 : bmr + 5;

    return Math.round(bmr);
  }, [userProfile]);

  // Połącz dane aktywności z workoutami
  const dailySummaries: DaySummary[] = useMemo(() => {
    return activityDays.reverse().map(day => {
      const dayWorkouts = workoutsByDate.get(day.date) || [];
      const workoutCalories = dayWorkouts.reduce((sum, w) => sum + (w.calories || 0), 0);

      return {
        date: day.date,
        activityCalories: Math.round(day.energyConsumption),
        bmrCalories: bmrPerDay,
        workoutCalories: Math.round(workoutCalories),
        totalCalories: Math.round(day.energyConsumption) + bmrPerDay, //+ Math.round(workoutCalories)
        workouts: dayWorkouts
      };
    });
  }, [activityDays, workoutsByDate, bmrPerDay]);

  if (isLoading) {
    return (
      <div className="flex justify-center py-4">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-4">
        <p className="text-red-500 text-sm mb-2">{error}</p>
        <button
          onClick={loadData}
          className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600"
        >
          {t('common.retry')}
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {t('suunto.dailySummary')}
      </h3>

      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="text-xs font-semibold text-gray-500 dark:text-gray-400 border-b border-gray-200 dark:border-gray-700">
              <th className="px-3 py-2 text-left">{t('suunto.date')}</th>
              <th className="px-3 py-2 text-right">🛌 {t('suunto.bmr')}</th>
              <th className="px-3 py-2 text-right">🚶 {t('suunto.activity')}</th>
              <th className="px-3 py-2 text-right">🏃 {t('suunto.workouts')}</th>
              <th className="px-3 py-2 text-right font-bold">🔥 {t('suunto.total')}</th>
              <th className="px-3 py-2 text-center">{t('suunto.workoutsCount')}</th>
            </tr>
          </thead>
          <tbody>
            {dailySummaries.slice(0, 10).map((summary) => (
              <tr
                key={summary.date}
                className="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                <td className="px-3 py-2 font-medium">{summary.date}</td>
                <td className="px-3 py-2 text-right text-gray-600 dark:text-gray-400">
                  {summary.bmrCalories.toLocaleString()}
                </td>
                <td className="px-3 py-2 text-right text-blue-600 dark:text-blue-400">
                  {summary.activityCalories.toLocaleString()}
                </td>
                <td className="px-3 py-2 text-right text-green-600 dark:text-green-400">
                  {summary.workoutCalories > 0 ? summary.workoutCalories.toLocaleString() : '-'}
                </td>
                <td className="px-3 py-2 text-right font-bold text-orange-600 dark:text-orange-400">
                  {summary.totalCalories.toLocaleString()}
                </td>
                <td className="px-3 py-2 text-center">
                  {summary.workouts.length > 0 ? (
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                      {summary.workouts.length}
                    </span>
                  ) : (
                    <span className="text-gray-400">-</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Szczegóły workoutów dla wybranych dni z workoutami */}
      {dailySummaries.filter(s => s.workouts.length > 0).slice(0, 3).map(summary => (
        <div key={summary.date} className="mt-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
          <h4 className="font-semibold text-gray-900 dark:text-white mb-2">
            {t('suunto.workouts')} - {summary.date}
          </h4>
          <div className="space-y-2">
            {summary.workouts.map(workout => (
              <div key={workout.activityId} className="flex justify-between items-center text-sm">
                <span className="text-gray-600 dark:text-gray-400">
                  {new Date(workout.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
                <span className="text-gray-900 dark:text-white">
                  {Math.floor(workout.totalTimeSeconds / 3600)}h {Math.floor((workout.totalTimeSeconds % 3600) / 60)}m
                </span>
                <span className="text-orange-600 dark:text-orange-400 font-medium">
                  {Math.round(workout.calories || 0)} kcal
                </span>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
};
