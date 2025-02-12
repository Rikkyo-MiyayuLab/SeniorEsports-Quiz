namespace UtilFuncs {
    public static class DateTimeUtils {
        public static int[] ConvertSecToHHMMSS(double sec) {
            // HHMMSS format initialization (all set to 0)
            int[] timeParts = new int[3] { 0, 0, 0 };

            // Calculate hours
            timeParts[0] = (int)(sec / 3600); // 3600 seconds = 1 hour
            sec %= 3600;

            // Calculate minutes
            timeParts[1] = (int)(sec / 60); // 60 seconds = 1 minute
            sec %= 60;

            // Calculate seconds
            timeParts[2] = (int)sec;

            return timeParts;
        }
    }
}