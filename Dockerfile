FROM openjdk:8
ARG SONAR_VERSION=7.7
ENV SONAR_VERSION=$SONAR_VERSION \
	SONARQUBE_HOME=/opt/sonarqube \
    # Database configuration
    # Defaults to using H2
    SONARQUBE_JDBC_USERNAME=sonar \
    SONARQUBE_JDBC_PASSWORD=sonar \
    SONARQUBE_JDBC_URL=

RUN echo "SONAR_VERSION is $SONAR_VERSION"

# Http port
EXPOSE 9000

RUN groupadd -r sonarqube && useradd -r -g sonarqube sonarqube

# grab gosu for easy step-down from root
RUN set -x \
    && wget -O /usr/local/bin/gosu "https://github.com/tianon/gosu/releases/download/1.10/gosu-$(dpkg --print-architecture)" \
    && chmod +x /usr/local/bin/gosu \
    && gosu nobody true

RUN set -x \
    && cd /opt \
    && curl -o sonarqube.zip -fSL https://binaries.sonarsource.com/Distribution/sonarqube/sonarqube-$SONAR_VERSION.zip \
    && unzip sonarqube.zip \
    && mv sonarqube-$SONAR_VERSION sonarqube \
	&& rm sonarqube.zip* \
	&& mv $SONARQUBE_HOME/lib/sonar-application-$SONAR_VERSION.jar $SONARQUBE_HOME/lib/sonar-application.jar \
	&& chown -R sonarqube:sonarqube sonarqube \
	&& chmod +x $SONARQUBE_HOME/lib/sonar-application.jar \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-csharp-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-css-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-flex-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-go-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-javascript-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-java-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-kotlin-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-ldap-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-php-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-python-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-ruby-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-typescript-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-scm-git-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-scm-svn-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-xml-plugin-* \
	&& rm $SONARQUBE_HOME/extensions/plugins/sonar-vbnet-plugin-* \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-css-plugin/sonar-css-plugin-1.0.3.724.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-html-plugin/sonar-html-plugin-3.1.0.1615.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-jacoco-plugin/sonar-jacoco-plugin-1.0.1.143.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-java-plugin/sonar-java-plugin-5.12.1.17771.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-javascript-plugin/sonar-javascript-plugin-5.1.1.7506.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-python-plugin/sonar-python-plugin-1.13.0.2922.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-ruby-plugin/sonar-ruby-plugin-1.5.0.315.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-scm-git-plugin/sonar-scm-git-plugin-1.8.0.1574.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-typescript-plugin/sonar-typescript-plugin-1.9.0.3766.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/SonarSource/sonar-dotnet/releases/download/7.14.0.8411/sonar-vbnet-plugin-7.14.0.8411.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-xml-plugin/sonar-xml-plugin-2.0.1.2020.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/hkamel/sonar-auth-aad/releases/download/1.1/sonar-auth-aad-plugin-1.1.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/galexandre/sonar-cobertura/releases/download/2.0/sonar-cobertura-plugin-2.0.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/QualInsight/qualinsight-plugins-sonarqube-smell/releases/download/qualinsight-plugins-sonarqube-smell-4.0.0/qualinsight-sonarqube-smell-plugin-4.0.0.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/DanielHWe/sonar-fxcop/releases/download/1.4.1_Release/sonar-fxcop-plugin-1.4.1.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/QualInsight/qualinsight-plugins-sonarqube-badges/releases/download/qualinsight-plugins-sonarqube-badges-3.0.1/qualinsight-sonarqube-badges-3.0.1.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/SonarSource/sonar-dotnet/releases/download/7.14.0.8411/sonar-csharp-plugin-7.14.0.8411.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/sbaudoin/sonar-yaml/releases/download/v1.4.2/sonar-yaml-plugin-1.4.2.jar \	
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/vaulttec/sonar-auth-oidc/releases/download/v1.0.4/sonar-auth-oidc-plugin-1.0.4.jar \
	&& wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/RIGS-IT/sonar-xanitizer/releases/download/2.0.0/sonar-xanitizer-plugin-2.0.0.jar \
	 && wget -P $SONARQUBE_HOME/extensions/plugins https://github.com/kwoding/sonar-webdriver-plugin/releases/download/sonar-webdriver-plugin-1.0.3/sonar-webdriver-plugin-1.0.3.jar \
	 && wget -P $SONARQUBE_HOME/extensions/plugins https://binaries.sonarsource.com/Distribution/sonar-kotlin-plugin/sonar-kotlin-plugin-1.5.0.315.jar \
	&& ls $SONARQUBE_HOME/lib \
	&& cd 	

#&& wget -P $SONARQUBE_HOME/extensions/plugins xxx \

VOLUME "$SONARQUBE_HOME/data"

RUN apt-get update && apt-get install -y dos2unix
COPY run.sh $SONARQUBE_HOME/bin/
RUN dos2unix $SONARQUBE_HOME/bin/run.sh && apt-get --purge remove -y dos2unix && rm -rf /var/lib/apt/lists/*
RUN chmod +x $SONARQUBE_HOME/bin/run.sh

WORKDIR $SONARQUBE_HOME
ENTRYPOINT ["./bin/run.sh"]
