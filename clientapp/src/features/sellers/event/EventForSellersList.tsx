import { observer } from "mobx-react";
import { Link } from "react-router-dom";
import { GridColumn, GridRow, ItemDescription, ItemHeader } from "semantic-ui-react";
import { Event } from "../../../app/models/tables/event";

interface Props {
    event: Event;
}

function EventForSellersList({ event }: Props) {

    return (
        <>
            <GridRow key={event.id}>
                <GridColumn width={4}>
                    <ItemHeader as={Link} to={`/event/${event.id}`}> {event.title} </ItemHeader>
                </GridColumn>
                <GridColumn width={9}>
                    <ItemDescription>Description: {event.description}</ItemDescription>
                </GridColumn>
            </GridRow>
        </>
    );
}

export default observer(EventForSellersList);
