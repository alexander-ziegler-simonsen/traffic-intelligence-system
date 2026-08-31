import { Slider, Text } from '@mantine/core';
import { useHover } from '@mantine/hooks';
import { useState } from 'react';

interface SliderProps {
    min: number;
    max: number;
}

export function SliderControl(props: SliderProps) {
    const [value, setValue] = useState(1);
    const { hovered, ref } = useHover();
    
    return (
        <div>
        <Text>value: {value}</Text>
        <Slider defaultValue={value} min={props.min} max={props.max} ref={ref} label={null} 
        thumbLabel="Hover slider" onChange={setValue}
        styles={{ thumb: { transition: 'opacity 150ms ease', opacity: hovered ? 1 : 0, }, }} />
        </div>
        
    );
}