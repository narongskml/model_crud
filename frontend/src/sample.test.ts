import { expect, test } from "bun:test";

test("math works", () => {
    expect(2 + 2).toBe(4);
});

test("frontend environment", () => {
    expect(process.env.NODE_ENV).toBeDefined();
});
