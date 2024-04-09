CC = gcc
CPPFLAGS = -MMD
# CFLAGS = -Wall -Wextra -g `pkg-config --cflags sdl2`
CFLAGS = -Wall -Wextra -g
LDFLAGS =
# LDLIBS = `pkg-config --libs sdl2`


SRC = chip.c
OBJ = ${SRC:.c=.o}
DEP = ${SRC:.c=.d}
EXE = ${SRC:.c=}

all: chip

chip: chip.o
chip.o: chip.c

-include ${DEP}

.PHONY: clean

clean:
	${RM} ${OBJ}
	${RM} ${DEP}

reset: clean
	${RM} ${EXE}

# END
