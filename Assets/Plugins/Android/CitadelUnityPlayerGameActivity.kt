package com.trioptimum.citadel

import android.os.Build
import com.unity3d.player.UnityPlayerGameActivity
import java.io.File

class CitadelUnityPlayerGameActivity : UnityPlayerGameActivity() {

    protected override fun updateUnityCommandLineArguments(cmdLine: String?): String? =
        if (preferVulkan()) appendCommandLineArgument(cmdLine, "-force-vulkan") else cmdLine

    private fun preferVulkan(): Boolean {
        var preferVulkanApi = false
        val pathToGraphicsApiFile =
            "${this.getExternalFilesDir("")!!.absolutePath}${File.separator}PreferVulkanApi.txt"
        val graphicApiFile = File(pathToGraphicsApiFile)
        if (graphicApiFile.exists()) {
            preferVulkanApi = graphicApiFile.readText().trim().toBoolean()
        }
        return preferVulkanApi && Build.VERSION.SDK_INT >= Build.VERSION_CODES.N
    }

    private fun appendCommandLineArgument(cmdLine: String?, arg: String?): String? {
        return if (arg.isNullOrEmpty()) cmdLine
        else if (cmdLine.isNullOrEmpty()) arg
        else "$cmdLine $arg"
    }
}