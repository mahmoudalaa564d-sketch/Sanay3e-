import { useEffect, useState } from "react";

import {
  MapContainer,
  TileLayer,
  Marker,
  useMapEvents,
  useMap,
} from "react-leaflet";

import L from "leaflet";
import { useFormContext, useWatch } from "react-hook-form";

import type { AddressFormValues } from "./AddressSchema";

// أيقونة الماركر
const customIcon = L.divIcon({
  className: "custom-marker",
  html: `
    <div style="
      width: 24px;
      height: 24px;
      background-color: #0966C1;
      border: 3px solid white;
      border-radius: 50%;
      box-shadow: 0 2px 8px rgba(0,0,0,0.3);
    "></div>
  `,
  iconSize: [24, 24],
  iconAnchor: [12, 12],
});

// مركز مصر الافتراضي
const DEFAULT_POSITION: [number, number] = [26.8206, 30.8025];

// استقبال الضغط على الخريطة
interface MapClickHandlerProps {
  onLocationSelect: (lat: number, lng: number) => void;
}

function MapClickHandler({
  onLocationSelect,
}: MapClickHandlerProps) {
  useMapEvents({
    click(e) {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });

  return null;
}

// تحريك الخريطة عند تغيير الموقع
interface MapUpdaterProps {
  position: [number, number] | null;
}

function MapUpdater({ position }: MapUpdaterProps) {
  const map = useMap();

  useEffect(() => {
    if (position) {
      map.flyTo(position, 16, {
        duration: 1.5,
      });
    }
  }, [map, position]);

  return null;
}

// المكون الرئيسي
export default function AddressLocationPicker() {
  const { control, setValue } = useFormContext<AddressFormValues>();

  const latitude = useWatch({
    control,
    name: "latitude",
  });

  const longitude = useWatch({
    control,
    name: "longitude",
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  // تحديد الموقع الحالي من قيم الفورم
  const position: [number, number] | null =
    latitude != null && longitude != null
      ? [latitude, longitude]
      : null;

  // تحديث الإحداثيات داخل React Hook Form
  const handleLocationSelect = (lat: number, lng: number) => {
    setValue("latitude", lat, {
      shouldValidate: true,
      shouldDirty: true,
    });

    setValue("longitude", lng, {
      shouldValidate: true,
      shouldDirty: true,
    });

    setError("");
  };

  // الحصول على الموقع الحالي من GPS
  const handleGetCurrentLocation = () => {
    if (!navigator.geolocation) {
      setError("المتصفح لا يدعم تحديد الموقع الجغرافي");
      return;
    }

    setIsLoading(true);
    setError("");

    navigator.geolocation.getCurrentPosition(
      (location) => {
        const lat = location.coords.latitude;
        const lng = location.coords.longitude;

        handleLocationSelect(lat, lng);

        setIsLoading(false);
      },

      (err) => {
        setIsLoading(false);

        switch (err.code) {
          case err.PERMISSION_DENIED:
            setError("يجب السماح للمتصفح بالوصول إلى موقعك");
            break;

          case err.POSITION_UNAVAILABLE:
            setError("تعذر تحديد موقعك الحالي");
            break;

          case err.TIMEOUT:
            setError("انتهى وقت محاولة تحديد الموقع");
            break;

          default:
            setError("حدث خطأ أثناء تحديد الموقع");
        }
      },

      {
        enableHighAccuracy: true,
        timeout: 15000,
        maximumAge: 0,
      }
    );
  };

  return (
    <div className="space-y-4">

      {/* العنوان والوصف */}
      <div>
        <h3 className="text-lg font-bold text-gray-800">
          تحديد موقع العنوان
        </h3>

        <p className="mt-1 text-sm text-gray-500">
          اضغط على الخريطة لتحديد موقع العنوان، أو استخدم موقعك الحالي.
        </p>
      </div>

      {/* زر تحديد الموقع الحالي */}
      <button
        type="button"
        onClick={handleGetCurrentLocation}
        disabled={isLoading}
        className="flex items-center gap-2 rounded-lg bg-[#0966C1] px-4 py-2.5 text-sm font-medium text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isLoading ? (
          <>
            <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
            جاري تحديد موقعك...
          </>
        ) : (
          <>
            <span>📍</span>
            تحديد موقعي الحالي
          </>
        )}
      </button>

      {/* رسالة الخطأ */}
      {error && (
        <p className="rounded-lg bg-red-50 p-3 text-sm text-red-600">
          {error}
        </p>
      )}

      {/* الخريطة */}
      <div className="h-[350px] w-full overflow-hidden rounded-xl border border-gray-200">
        <MapContainer
          center={position ?? DEFAULT_POSITION}
          zoom={position ? 16 : 6}
          scrollWheelZoom
          className="h-full w-full"
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          <MapClickHandler
            onLocationSelect={handleLocationSelect}
          />

          <MapUpdater position={position} />

          {position && (
            <Marker
              position={position}
              icon={customIcon}
            />
          )}
        </MapContainer>
      </div>

      {/* حالة الموقع */}
      {position ? (
        <div className="rounded-lg bg-green-50 p-3 text-sm text-green-700">
          <p className="font-semibold">
            تم تحديد موقع العنوان بنجاح
          </p>

          <p className="mt-1" dir="ltr">
            Latitude: {position[0].toFixed(6)}
          </p>

          <p dir="ltr">
            Longitude: {position[1].toFixed(6)}
          </p>
        </div>
      ) : (
        <p className="text-sm text-gray-500">
          لم يتم تحديد الموقع بعد.
        </p>
      )}

    </div>
  );
}