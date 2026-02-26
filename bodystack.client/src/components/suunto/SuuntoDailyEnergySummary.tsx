import React from 'react';
import { useTranslation } from 'react-i18next';

interface SuuntoDailyEnergySummaryProps {
  bmrCalories: number;
  activityCalories: number;
  workoutCalories: number;
}

export const SuuntoDailyEnergySummary: React.FC<SuuntoDailyEnergySummaryProps> = ({
  bmrCalories,
  activityCalories,
  workoutCalories
}) => {
  const { t } = useTranslation();
  
  const totalCalories = bmrCalories + activityCalories + workoutCalories;
  
  const formatCalories = (calories: number): string => {
    return Math.round(calories).toLocaleString();
  };
  
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
        {t('suunto.dailySummary')}
      </h3>
      
      <div className="space-y-3">
        {/* BMR */}
        <div className="flex justify-between items-center py-2 border-b border-gray-100 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <span className="text-xl">🛌</span>
            <span className="text-gray-700 dark:text-gray-300">
              {t('suunto.bmr')}
            </span>
          </div>
          <span className="font-semibold text-gray-900 dark:text-white">
            {formatCalories(bmrCalories)} kcal
          </span>
        </div>
        
        {/* Activity */}
        <div className="flex justify-between items-center py-2 border-b border-gray-100 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <span className="text-xl">🚶</span>
            <span className="text-gray-700 dark:text-gray-300">
              {t('suunto.activity')}
            </span>
          </div>
          <span className="font-semibold text-blue-600 dark:text-blue-400">
            {formatCalories(activityCalories)} kcal
          </span>
        </div>
        
        {/* Workouts */}
        <div className="flex justify-between items-center py-2 border-b border-gray-100 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <span className="text-xl">🏃</span>
            <span className="text-gray-700 dark:text-gray-300">
              {t('suunto.workouts')}
            </span>
          </div>
          <span className="font-semibold text-green-600 dark:text-green-400">
            {formatCalories(workoutCalories)} kcal
          </span>
        </div>
        
        {/* Total */}
        <div className="flex justify-between items-center pt-3 mt-3 border-t-2 border-gray-200 dark:border-gray-600">
          <div className="flex items-center gap-2">
            <span className="text-xl">🔥</span>
            <span className="font-bold text-gray-900 dark:text-white text-lg">
              {t('suunto.total')}
            </span>
          </div>
          <span className="font-bold text-orange-600 dark:text-orange-400 text-xl">
            {formatCalories(totalCalories)} kcal
          </span>
        </div>
      </div>
    </div>
  );
};
