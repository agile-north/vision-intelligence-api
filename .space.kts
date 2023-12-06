import java.io.File
job("Build and push Docker :Vision Intelligence API") {
    parameters {
        secret("nuget-proget-username","{{ project:nuget--proget--username }}", allowCustomRunOverride = true)
        secret("nuget-proget-password","{{ project:nuget--proget--password }}", allowCustomRunOverride = true)
        text("assembly-version-path","VersionInfo.cs")
        text("nuget-config-path","NuGet.Config")
        text("dockerfile-path","API/Dockerfile")
        text("package-repo-name","containers")
    }
    container(displayName = "Extract assembly version", image = "ubuntu") {
        shellScript {
            interpreter = "/bin/bash"
            content = """
                grep -E 'assembly: AssemblyFileVersion' ${'$'}JB_SPACE_WORK_DIR_PATH/{{ assembly-version-path }} \
                 | grep -E -o '[0-9]+' \
                 | awk -v d="." '{s=(NR==1?s:s d)${'$'}0}END{print s}' \
                 | cat > ${'$'}JB_SPACE_FILE_SHARE_PATH/version.txt
            """.trimIndent()
        }
    }
    container(displayName = "Establish assembly version","amazoncorretto:17-alpine") {
        kotlinScript { api ->
           val imageTag = if (api.gitBranch() == "refs/heads/main") {
                "release"
            } else {
                api.gitBranch()
                        .replace("refs/heads/", "")
                        .replace("/", "-")
            }
            api.parameters["vcs-branch"] = imageTag
            val version:String =  api.fileShare().locate("version.txt")!!.readText(Charsets.UTF_8)
            api.parameters["sc-version"] = version.trim()
            val parts = version.split('.')
            val versionFull = parts[0] + "." + parts[1] + "." + api.executionNumber().toString()
            api.parameters["Version.Major"]= parts[0]
            api.parameters["Version.Minor"]=parts[1]
            api.parameters["Version.Full"]= versionFull
            api.parameters["Version.Full.Assembly"]= "$versionFull.0"
        }
    }
    container(displayName = "Update assembly version", image = "ubuntu") {
        shellScript {
            interpreter = "/bin/bash"
            content = """
                sed -i 's/{{sc-version}}/{{Version.Full.Assembly}}/g' ${'$'}JB_SPACE_WORK_DIR_PATH/{{ assembly-version-path }} \
                && cp ${'$'}JB_SPACE_WORK_DIR_PATH/{{ assembly-version-path }} ${'$'}JB_SPACE_FILE_SHARE_PATH/VersionInfo.cs
            """.trimIndent()
        }
    }
    container(displayName = "Update nuget config", image = "ubuntu") {
        env["NUGET_PROGET_USERNAME"] = "{{ nuget-proget-username }}"
        env["NUGET_PROGET_PASSWORD"] = "{{ nuget-proget-password }}"
        shellScript {
            interpreter = "/bin/bash"
            content = """
                sed -i \
                 "s/%nuget__proget__username%/${'$'}NUGET_PROGET_USERNAME/g" \
                 ${'$'}JB_SPACE_WORK_DIR_PATH/{{ nuget-config-path }} && \
                 sed -i \
                 "s/%nuget__proget__password%/${'$'}NUGET_PROGET_PASSWORD/g" \
                 ${'$'}JB_SPACE_WORK_DIR_PATH/{{ nuget-config-path }} && \
                 cp ${'$'}JB_SPACE_WORK_DIR_PATH/{{ nuget-config-path }} ${'$'}JB_SPACE_FILE_SHARE_PATH/NuGet.Config
            """.trimIndent()
        }
    }
    container(displayName = "Establish repository","amazoncorretto:17-alpine") {
        kotlinScript { api ->
            val company = api.spaceUrl().split('.')[0].replace("https://","").trim()
            val project = api.projectKey()
            val repository = api.gitRepositoryName()
            api.parameters["SpaceRepo"] = "$company.registry.jetbrains.space/p/nrth/{{ package-repo-name }}/$repository".lowercase()
        }
    }
    host("Build and publish Docker image") {
        shellScript {
            interpreter = "/bin/bash"
            content = """
                cp ${'$'}JB_SPACE_FILE_SHARE_PATH/VersionInfo.cs ${'$'}JB_SPACE_WORK_DIR_PATH/{{ assembly-version-path }} && \
                cp ${'$'}JB_SPACE_FILE_SHARE_PATH/NuGet.Config ${'$'}JB_SPACE_WORK_DIR_PATH/{{ nuget-config-path }}
            """.trimIndent()
        }
        dockerBuildPush {
            // Docker context, by default, project root
            context = "."
            // path to Dockerfile relative to project root
            // if 'file' is not specified, Docker will look for it in 'context'/Dockerfile
            file = "{{ dockerfile-path }}"
            labels["vendor"] = "agile-nrth"
            // image tags for 'docker push'
            tags {
                +"{{SpaceRepo}}:{{ Version.Full }}-{{ vcs-branch }}"
                +"{{SpaceRepo}}:{{ Version.Major }}.{{ Version.Minor }}-{{ vcs-branch }}"
                +"{{SpaceRepo}}:{{ Version.Major }}-{{ vcs-branch }}"
            }
        }
    }
}