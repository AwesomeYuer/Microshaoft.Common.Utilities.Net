namespace Microshaoft
{
    using System;
    public static class LockObjectHelper
    {
        public static void LockIf<T>
                            (
                                this T @this
                                , Func<bool> onEnterLockPredictProcessFunc
                                , Action onLockingProcessAction
                            )
        {
            if (onEnterLockPredictProcessFunc())
            {
                lock (@this)
                {
                    if (onEnterLockPredictProcessFunc())
                    {
                        onLockingProcessAction();
                    }
                }
            }
        }
    
        public static void LockIf
                            (
                                this object @this
                                , Func<bool> onEnterLockPredictProcessFunc
                                , Action onLockingProcessAction
                            )
        {
            LockIf<object>
                        (
                            @this
                            , onEnterLockPredictProcessFunc
                            , onLockingProcessAction
                        );
        }
    }
}
