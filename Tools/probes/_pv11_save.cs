string r = "";
int p0 = ForgeState.Pity; ForgeState.Pity = 7; r += "set7->" + ForgeState.Pity;
ES3.DeleteKey("forge.pity"); r += " deleted->" + ForgeState.Pity;
ForgeState.Pity = p0; r += " restored->" + ForgeState.Pity + " (orig " + p0 + ")";
int u0 = ForgeState.UnlockMask; ForgeState.UnlockMask = 5; r += " unlock5->" + ForgeState.UnlockMask; ES3.DeleteKey("forge.unlock"); r += " del->" + ForgeState.UnlockMask; ForgeState.UnlockMask = u0;
r += " done";
return r;
