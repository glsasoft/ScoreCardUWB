package cn.com.glsa.app.fightgame.oad;

import static cn.com.glsa.app.fightgame.oad.OAD.HAL_FLASH_WORD_SIZE;
import static cn.com.glsa.app.fightgame.oad.OAD.OAD_BLOCK_SIZE;

public class ProgInfo {
        int iBytes = 0; // Number of bytes programmed
        short iBlocks = 0; // Number of blocks programmed
        short nBlocks = 0; // Total number of blocks
        int iTimeElapsed = 0; // Time elapsed in milliseconds
        int mTick = 0;

        void reset(ImgHdr fileImgHdr) {
            iBytes = 0;
            iBlocks = 0;
            iTimeElapsed = 0;
            mTick = 0;
            nBlocks = (short) (fileImgHdr.len / (OAD_BLOCK_SIZE / HAL_FLASH_WORD_SIZE));
        }
    }