#ifndef LOG_H 
#define LOG_H

#include <err.h>
#include <stdio.h>

#ifndef NOLOG
#define LOG(...) \
    fprintf(stderr, "[LOG] "); \
    warn(__VA_ARGS__);

#define LOGX(...) \
    fprintf(stderr, "[LOG] "); \
    warnx(__VA_ARGS__);
#endif

#ifndef NOWARN
#define WARN(...) \
    fprintf(stderr, "[WARNING] at '%s', '%s'\n-> ", __FILE__, __func__); \
    warn(__VA_ARGS__);

#define WARNX(...) \
    fprintf(stderr, "[WARNING] at '%s', '%s'\n-> ", __FILE__, __func__); \
    warnx(__VA_ARGS__);
#endif

#ifndef NOERR
#define ERR(...) \
    fprintf(stderr, "[ERROR] at '%s', '%s'\n-> ", __FILE__, __func__); \
    err(__VA_ARGS__);

#define ERRX(...) \
    fprintf(stderr, "[ERROR] at '%s', '%s'\n-> ", __FILE__, __func__); \
    errx(__VA_ARGS__);
#endif

#endif /* ! */
