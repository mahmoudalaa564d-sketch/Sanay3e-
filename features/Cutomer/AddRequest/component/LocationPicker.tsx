import { useState } from "react";
import {
  MapContainer,
  TileLayer,
  Marker,
  useMapEvents,
  useMap,
} from "react-leaflet";

import { useFormContext } from "react-hook-form";
import L from "leaflet";

import type { ServiceRequestFormData } from "./ServiceRequestSchema";


// Marker Icon
const markerIcon = L.divIcon({
  className: "",
  html: `
    <div style="
      width: 22px;
      height: 22px;
      background: #0966C1;
      border: 4px solid white;
      border-radius: 50%;
      box-shadow: 0 2px 8px rgba(0,0,0,0.4);
    "></div>
  `,
  iconSize: [22, 22],
  iconAnchor: [11, 11],
});

// Map Click Handler
const MapClickHandler = ({
  onLocationSelect,
}: {
  onLocationSelect: (lat: number, lng: number) => void;
}) => {
  useMapEvents({
    click(e) {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });

  return null;
};

// تحريك الخريطة للموقع الجديد
const MapUpdater = ({
  position,
}: {
  position: [number, number] | null;
}) => {
  const map = useMap();

  if (position) {
    map.flyTo(position, 16, {
      duration: 1.5,
    });
  }

  return null;
};

const LocationPicker = () => {
  const { setValue, watch } =
    useFormContext<ServiceRequestFormData>();

  const latitude = watch("latitude");
  const longitude = watch("longitude");

  const [position, setPosition] = useState<[number, number] | null>(
    latitude != null && longitude != null
      ? [latitude, longitude]
      : null
  );

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  // حفظ الموقع
  const handleLocationSelect = (lat: number, lng: number) => {
    setPosition([lat, lng]);

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

  // تحديد الموقع الحالي
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

      (error) => {
        setIsLoading(false);

        switch (error.code) {
          case error.PERMISSION_DENIED:
            setError("يجب السماح للمتصفح بالوصول إلى موقعك");
            break;

          case error.POSITION_UNAVAILABLE:
            setError("تعذر تحديد موقعك الحالي");
            break;

          case error.TIMEOUT:
            setError("انتهى وقت محاولة تحديد الموقع، حاول مرة أخرى");
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
    <div className="mt-6">

      <div className="mb-4">
        <h3 className="mb-2 text-lg font-bold text-gray-800">
          حدد موقع تنفيذ الخدمة
        </h3>

        <p className="text-sm text-gray-500">
          يمكنك تحديد موقعك تلقائيًا أو اختيار المكان من الخريطة.
        </p>
      </div>

      {/* Current Location Button */}
      <button
        type="button"
        onClick={handleGetCurrentLocation}
        disabled={isLoading}
        className="mb-4 flex w-full items-center justify-center gap-2 rounded-lg bg-[#0966C1] px-5 py-3 font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60 md:w-fit"
      >
        {isLoading ? (
          <>
            <span className="h-5 w-5 animate-spin rounded-full border-2 border-white border-t-transparent" />
            جاري تحديد موقعك...
          </>
        ) : (
          <>
            <span>📍</span>
            تحديد موقعي الحالي
          </>
        )}
      </button>

      {/* Error */}
      {error && (
        <p className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
          {error}
        </p>
      )}

      {/* Map */}
      <div className="overflow-hidden rounded-xl border border-gray-200">
        <MapContainer
          center={position ?? [26.8206, 30.8025]}
          zoom={position ? 16 : 6}
          scrollWheelZoom
          className="h-[350px] w-full"
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
            <Marker position={position} icon={markerIcon} />
          )}
        </MapContainer>
      </div>

      {/* Selected Location */}
      <div className="mt-3 rounded-lg bg-blue-50 p-3 text-sm">

        {position ? (
          <p className="font-medium text-blue-800">
            ✓ تم تحديد موقع تنفيذ الخدمة بنجاح
          </p>
        ) : (
          <p className="text-gray-600">
            لم يتم تحديد الموقع بعد
          </p>
        )}

      </div>

    </div>
  );
};

export default LocationPicker;