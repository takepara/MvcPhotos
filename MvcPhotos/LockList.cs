using System.Collections.Generic;

namespace MvcPhotos
{
    public class LockList<T>
    {
        private List<T> _lock = new List<T>();
        public bool Lock(T value)
        {
            if (_lock.Contains(value))
                return false;

            lock(_lock)
            {
                if (_lock.Contains(value))
                    return false;

                _lock.Add(value);
            }
            return true;
        }

        public void Unlock(T value)
        {
            if (!_lock.Contains(value)) return;
            lock (_lock)
            {
                if (!_lock.Contains(value))
                    return;

                _lock.Remove(value);
            }
        }
    }
}