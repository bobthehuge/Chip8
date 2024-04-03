#include <stdio.h>
#include <stdlib.h>

#define MEM_SIZE 4096
#define ROM "roms/trip8.ch8"

struct Chip {
    char reg[16];
    char sound;
    char timer;
    char pc;
    char mem[MEM_SIZE];
};



int main(void)
{
    struct Chip chip = {0};

    return 0;
}
