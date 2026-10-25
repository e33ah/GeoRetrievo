import { describe, expect, it } from "vitest";
import { getRegionClass } from "./formatters";

describe("getRegionClass", () => {
  it.each(["Americas", "North America", "South America"])(
    "uses the Americas badge style for %s",
    (region) => {
      expect(getRegionClass(region)).toBe("badge-region-americas");
    },
  );
});
