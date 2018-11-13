namespace Microshaoft
{
    using System;
    public static class LockObjectHelper
    {
        public static void LockIf<T>
                            (
                                this T lockerObject
                                , Func<bool> onEnterLockPredictProcessFunc
                                , Action onLockingProcessAction
                            )
        {
            if (onEnterLockPredictProcessFunc())
            {
                lock (lockerObject)
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
                                this object target
                                , Func<bool> onEnterLockPredictProcessFunc
                                , Action onLockingProcessAction
                            )
        {
            LockIf<object>
                        (
                            target
                            , onEnterLockPredictProcessFunc
                            , onLockingProcessAction
                        );
        }
    }
}
