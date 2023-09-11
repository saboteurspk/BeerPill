#include <Arduino.h>
#include <ArduinoJson.h>
#include <HTTPClient.h>
#include <ArduinoOTA.h>
#include "WiFi.h"
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h> // Only needed for Arduino 1.6.5 and earlier

Adafruit_MPU6050 mpu;

bool initWiFi()
{
    WiFi.mode(WIFI_STA);
    WiFi.begin("Wi-Fi SSID", "Wi-Fi password");
    Serial.print("Connecting to WiFi");
    long int t1 = millis();

    do
    {
        delay(300);
        Serial.print('.');

    } while (WiFi.status() != WL_CONNECTED && millis() - t1 < 30000);

    if (WiFi.status() == WL_CONNECTED)
    {
        Serial.println("Connected");
    }
    else
    {
        Serial.println("Connection timeout");
    }

    return WiFi.status() == WL_CONNECTED;
}

void setup()
{
    Serial.begin(115200);

    Serial.println("System start");
    Wire.begin(21, 22);

    if (!mpu.begin())
    {
        Serial.println("Failed to find MPU6050 chip");
    }

    Serial.println("MPU6050 Found!");
    mpu.enableSleep(false);
    mpu.setGyroStandby(true, true, true);

    mpu.setFilterBandwidth(MPU6050_BAND_5_HZ);

    bool connected = initWiFi();
    // if (!connected) {
    //   ESP.restart();
    // }

    Serial.println("Ready");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());

    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);

    mpu.enableSleep(true);

    Serial.print("Acc ");
    Serial.print("X: ");
    Serial.print(a.acceleration.x, 2);
    Serial.print(" m/s^2, ");
    Serial.print("Y: ");
    Serial.print(a.acceleration.y, 2);
    Serial.print(" m/s^2, ");
    Serial.print("Z: ");
    Serial.print(a.acceleration.z, 2);
    Serial.println(" m/s^2");

    Serial.print("Temperature: ");
    Serial.print(temp.temperature, 2);
    Serial.println(" degC");

    HTTPClient http;
    http.begin("http://yourservername/api/data/savedata");
    http.addHeader("Content-Type", "application/json");

    StaticJsonDocument<200> doc;

    doc["X"] = a.acceleration.x;
    doc["Y"] = a.acceleration.y;
    doc["Z"] = a.acceleration.z;
    doc["T"] = temp.temperature;
    doc["DeviceId"] = "pill1";
    doc["Key"] = "pill1 Key";

    String requestBody;
    serializeJson(doc, requestBody);

    int httpResponseCode = http.POST(requestBody);

    if (httpResponseCode > 0)
    {
        Serial.print("odeslano ");
        Serial.println(httpResponseCode);
    }
    else
    {
        Serial.printf("Error occurred while sending HTTP POST: %s\n", http.errorToString(httpResponseCode).c_str());
    }

    Serial.flush();

    esp_sleep_pd_config(ESP_PD_DOMAIN_RTC_PERIPH, ESP_PD_OPTION_OFF);
    esp_sleep_pd_config(ESP_PD_DOMAIN_RTC_SLOW_MEM, ESP_PD_OPTION_OFF);
    esp_sleep_pd_config(ESP_PD_DOMAIN_RTC_FAST_MEM, ESP_PD_OPTION_OFF);
    esp_sleep_pd_config(ESP_PD_DOMAIN_XTAL, ESP_PD_OPTION_OFF);

    // esp_sleep_enable_timer_wakeup(30000000); // 30s
    esp_sleep_enable_timer_wakeup(3600000000); // 1H

    esp_deep_sleep_start();
}

void loop()
{
}
