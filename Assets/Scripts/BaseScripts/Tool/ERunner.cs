using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace LetsPiu
{
    public sealed class ERunner
    {
        private ERunner()
        {

        }
        public static bool Run(Action run)
        {
            try
            {
                run();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool Run(out Exception except, Action run)
        {
            try
            {
                run();
            }
            catch (Exception ex)
            {
                except = ex;
                Debug.LogException(ex);
                return (false);
            }

            except = null;

            return (true);
        }

        public static bool Run<T>(Action<T> run, T p)
        {
            try
            {
                run(p);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool Run<T1, T2>(Action<T1, T2> run, T1 p1, T2 p2)
        {
            try
            {
                run(p1, p2);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool Run<T1, T2, T3>(Action<T1, T2, T3> run, T1 p1, T2 p2, T3 p3)
        {
            try
            {
                run(p1, p2, p3);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool Run<T>(out Exception except, Action<T> run, T p)
        {
            try
            {
                run(p);
            }
            catch (Exception ex)
            {
                except = ex;
                Debug.LogException(ex);
                return false;
            }

            except = null;

            return true;
        }

        public static bool Run<T1, T2>(out Exception except, Action<T1, T2> run, T1 p1, T2 p2)
        {
            try
            {
                run(p1, p2);
            }
            catch (Exception ex)
            {
                except = ex;
                Debug.LogException(ex);
                return false;
            }

            except = null;

            return true;
        }

        public static bool Run<T1, T2, T3>(out Exception except, Action<T1, T2, T3> run, T1 p1, T2 p2, T3 p3)
        {
            try
            {
                run(p1, p2, p3);
            }
            catch (Exception ex)
            {
                except = ex;
                Debug.LogException(ex);
                return false;
            }

            except = null;

            return true;
        }
    }
}
